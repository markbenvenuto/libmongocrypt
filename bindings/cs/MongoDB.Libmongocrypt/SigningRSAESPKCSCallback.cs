/*
 * Copyright 2020–present MongoDB, Inc.
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
#if NETSTANDARD2_1
using System.Security.Cryptography;
#endif

namespace MongoDB.Libmongocrypt
{
    internal static class SigningRSAESPKCSCallback
    {
#pragma warning disable IDE1006 // Naming Styles
        public static bool rsaSign(
#pragma warning restore IDE1006 // Naming Styles
            IntPtr ctx,
            IntPtr key,
            IntPtr inData,
            IntPtr outData,
            IntPtr statusPtr)
        {
            Status status = new Status(StatusSafeHandle.FromIntPtr(statusPtr));
            try
            {
                Binary keyBinary = new Binary(BinarySafeHandle.FromIntPtr(key));
                Binary inBinary = new Binary(BinarySafeHandle.FromIntPtr(inData));
                Binary outBinary = new Binary(BinarySafeHandle.FromIntPtr(outData));

                byte[] originalData = inBinary.ToArray();
                byte[] keyBytes = keyBinary.ToArray();
                byte[] signedData;

                // Hash and sign the data.
                signedData = HashAndSignBytes(originalData, keyBytes);

                outBinary.WriteBytes(signedData);
            } catch (Exception e)
            {
                status.SetStatus(1, e.Message);
                return false;
            }

            return true;
        }

        public static byte[] HashAndSignBytes(byte[] dataToSign, byte[] key)
        {
#if NETSTANDARD2_1
            try
            {
                var rsaProvider = new RSACryptoServiceProvider();

                rsaProvider.ImportPkcs8PrivateKey(key, out _);

                return rsaProvider.SignData(dataToSign, SHA256.Create());
            }
            catch (CryptographicException)
            {
                throw;
            }
#else
            throw new System.PlatformNotSupportedException("RSACryptoServiceProvider.ImportPkcs8PrivateKey is supported only on frameworks higher or equal to .netstandard2.1.");
#endif
        }
    }
}
