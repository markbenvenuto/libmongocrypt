/*
 * Copyright 2019-present MongoDB, Inc.
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
    /// <summary>
    /// CryptClient represents a session with libmongocrypt.
    /// 
    /// It can be used to encrypt and decrypt documents.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class CryptClient : IDisposable
    {
        private MongoCryptSafeHandle _handle;

        internal CryptClient(MongoCryptSafeHandle handle)
        {
            _handle = handle;
        }

        /// <summary>
        /// Starts the encryption context.
        /// </summary>
        /// <param name="ns">The namespace of the collection.</param>
        /// <param name="keyId">The key identifier.</param>
        /// <returns>A encryption context</returns>
        public CryptContext StartEncryptionContext(string ns, IKmsKeyId keyId)
        {
            ContextSafeHandle handle = Library.mongocrypt_ctx_new(_handle);

            IntPtr stringPointer = (IntPtr)Marshal.StringToHGlobalAnsi(ns);

            try
            {
                Library.mongocrypt_ctx_encrypt_init(handle, stringPointer, Strlen(stringPointer));
            }
            finally
            {
                Marshal.FreeHGlobal(stringPointer);
            }

            return new CryptContext(handle);
        }

        /// <summary>
        /// Starts the decryption context.
        /// </summary>
        /// <param name="buffer">The bson document to decrypt.</param>
        /// <param name="keyId">The key identifier.</param>
        /// <returns>A decryption context</returns>
        public CryptContext StartDecryptionContext(byte[] buffer, IKmsKeyId keyId)
        {
            ContextSafeHandle handle = Library.mongocrypt_ctx_new(_handle);

            GCHandle gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            PinnedBinary pinned = new PinnedBinary(gch.AddrOfPinnedObject(), (uint)buffer.Length);

            Library.mongocrypt_ctx_decrypt_init(handle, pinned.Handle);

            return new CryptContext(handle, gch, pinned);
        }

        internal static uint Strlen(IntPtr ptr)
        {
            uint count = 0;
            unsafe
            {
                byte* p = (byte*)ptr.ToPointer();
                while (*p != 0)
                {
                    count++;
                    p++;
                }
            }

            return count;
        }

        internal void SetAwsCredentials(ContextSafeHandle handle, string region, string customerMasterKey)
        {
            IntPtr stringPointer = (IntPtr)Marshal.StringToHGlobalAnsi(region);

            try
            {
                IntPtr keyPointer = (IntPtr)Marshal.StringToHGlobalAnsi(region);

                try
                {
                    // TODO - check ret
                    Library.mongocrypt_ctx_setopt_masterkey_aws(handle, stringPointer, Strlen(stringPointer), keyPointer, Strlen(keyPointer));
                }
                finally
                {
                    Marshal.FreeHGlobal(keyPointer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(stringPointer);
            }
        }

        internal void InitKey(ContextSafeHandle handle, IKmsKeyId keyId)
        {
            switch (keyId.KeyType)
            {
                case KmsKeyType.Aws:
                    var aws = (AwsKeyId)keyId;
                    SetAwsCredentials(handle, aws.Region, aws.CustomerMasterKey);
                    break;
                default:
                    throw new NotImplementedException();
            }
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

    }
}
