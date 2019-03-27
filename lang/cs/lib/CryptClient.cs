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

    public class ReadOnlyBinary : IDisposable
    {
        internal ReadOnlyBinary()
        {
            _handle = Library.mongocrypt_binary_new();
        }

        IntPtr Data
        {
            get { return Library.mongocrypt_binary_data(_handle); }
        }

        UInt32 Length
        {
            get { return Library.mongocrypt_binary_len(_handle); }
        }

        byte[] ToArray()
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

    public class CryptContext : IDisposable
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
        }

        StateCode State
        {
            get
            {
                return Library.mongocrypt_ctx_state(_handle);
            }
        }

        bool IsDone
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

        ContextSafeHandle _handle;
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
        }

        public CryptContext StartEncryptionContext(string ns)
        {
            ContextSafeHandle handle = Library.mongocrypt_ctx_new(_handle);

            IntPtr stringPointer = (IntPtr)Marshal.StringToHGlobalAnsi(ns);

            Library.mongocrypt_ctx_encrypt_init(handle, stringPointer, Strlen(stringPointer);

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