/*
 * Copyright 2018-present MongoDB, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Runtime.InteropServices;

namespace MongoDB.Crypt
{
    public class Binary : IDisposable
    {
        internal Binary()
        {
            _handle = Library.mongocrypt_binary_new();
        }

        internal Binary(BinarySafeHandle handle)
        {
            _handle = handle;
        }

        public IntPtr Data
        {
            get { return Library.mongocrypt_binary_data(_handle); }
        }

        public UInt32 Length
        {
            get { return Library.mongocrypt_binary_len(_handle); }
        }

        internal BinarySafeHandle Handle => _handle;

        public byte[] ToArray()
        {
            byte[] arr = new byte[Length];
            Marshal.Copy(Data, arr, 0, arr.Length);
            return arr;
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_handle.IsClosed)
            {
                _handle.Dispose();
            }
        }
        #endregion

        BinarySafeHandle _handle;
    }

    public class PinnedBinary : Binary
    {
        internal PinnedBinary(IntPtr ptr, UInt32 len) : base(Library.mongocrypt_binary_new_from_data(ptr, len))
        {
        }
    }

    //public class BinaryArray : Binary
    //{
    //    internal BinaryArray(byte[] array) : base(Library.mongocrypt_binary_new_from_data())
    //    {
    //    }
    //}

    public class CryptContext : IDisposable, IStatus
    {
        public enum StateCode
        {
            MONGOCRYPT_CTX_ERROR = 0,
            MONGOCRYPT_CTX_NOTHING_TO_DO = 1,
            MONGOCRYPT_CTX_NEED_MONGO_COLLINFO = 2, /* run on main MongoClient */
            MONGOCRYPT_CTX_NEED_MONGO_MARKINGS = 3, /* run on mongocryptd. */
            MONGOCRYPT_CTX_NEED_MONGO_KEYS = 4,     /* run on key vault */
            MONGOCRYPT_CTX_NEED_KMS = 5,
            MONGOCRYPT_CTX_READY = 6, /* ready for encryption/decryption */
            MONGOCRYPT_CTX_DONE = 7
        };

        internal CryptContext(ContextSafeHandle handle)
        {
            _handle = handle;
            _status = new Status();
        }

        public StateCode State
        {
            get
            {
                return Library.mongocrypt_ctx_state(_handle);
            }
        }

        public bool IsDone
        {
            get { return this.State == StateCode.MONGOCRYPT_CTX_DONE; }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_handle.IsClosed)
            {
                _handle.Dispose();
            }
        }
        #endregion

        public Binary GetOperation()
        {
            Binary binary = new Binary();
            Check(Library.mongocrypt_ctx_mongo_op(_handle, binary.Handle));
            return binary;
        }

        public void Feed(byte[] buffer)
        {
            unsafe
            {
                fixed (byte* p = buffer)
                {
                    IntPtr ptr = (IntPtr)p;
                    using(PinnedBinary pinned = new PinnedBinary(ptr, (UInt32)buffer.Length))
                    {
                        Check(Library.mongocrypt_ctx_mongo_op(_handle, pinned.Handle));
                    }
                }
            }
        }



        public void MarkDone()
        {
            bool done = Library.mongocrypt_ctx_mongo_done(_handle);
            // TODO - check done
        }

        void Check(bool ret)
        {
            if(!ret)
            {
                _status.Check(this);
            }
        }

        void IStatus.Check(Status status)
        {
            Library.mongocrypt_ctx_status(_handle, status.Handle);
        }

        ContextSafeHandle _handle;
        private Status _status;
    }

    public class CryptClient : IDisposable
    {
        internal CryptClient(MongoCryptSafeHandle handle)
        {
            this._handle = handle;
        }

        public void Foo()
        {
            Console.WriteLine("Hi");
        }

        private UInt32 Strlen(IntPtr ptr)
        {
            UInt32 count = 0;
            unsafe
            {
                byte* p = (byte*)ptr.ToPointer();
                while(*p == 0)
                {
                    count++;
                    p++;
                }
            }

            return count;
        }

        public CryptContext StartEncryptionContext(string ns)
        {
            ContextSafeHandle handle = Library.mongocrypt_ctx_new(_handle);

            IntPtr stringPointer = (IntPtr)Marshal.StringToHGlobalAnsi(ns);

            Library.mongocrypt_ctx_encrypt_init(handle, stringPointer, Strlen(stringPointer));

            Marshal.FreeHGlobal(stringPointer);

            return new CryptContext(handle);

        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(_handle.IsClosed)
            {
                _handle.Dispose();
            }
        }
        #endregion
        MongoCryptSafeHandle _handle;
    }
}