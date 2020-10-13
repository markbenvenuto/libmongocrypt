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
using System.Security.Cryptography;

namespace MongoDB.Libmongocrypt
{
    internal static class SigningRSAESPKCSCallback
    {
#pragma warning disable IDE1006 // Naming Styles
        public static bool hmac(
#pragma warning restore IDE1006 // Naming Styles
            IntPtr ctx,
            BinarySafeHandle key,
            BinarySafeHandle @in,
            BinarySafeHandle @out,
            StatusSafeHandle status)
        {
#if NET452
            byte[] originalData = GetBytes(@in);
            byte[] keyBytes = GetBytes(key);
            byte[] signedData;

            // Hash and sign the data.
            signedData = HashAndSignBytes(originalData, keyBytes);
#endif
            return true;
        }

        public static byte[] HashAndSignBytes(byte[] dataToSign, byte[] key /*RSAParameters Key*/)
        {
#if NET452
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the
                // key from RSAParameters.
                var rsaProvider = new RSACryptoServiceProvider(); // RSAalg

                //rsaProvider.ImportParameters(key);

                // Hash and sign the data. Pass a new instance of SHA256
                // to specify the hashing algorithm.
                return rsaProvider.SignData(dataToSign, SHA256.Create());
            }
            catch (CryptographicException)
            {
                throw;
            }
#else
            throw new System.NotSupportedException("RSACryptoServiceProvider is supported only on net452.");
#endif
        }

        // private methods
        private static byte[] GetBytes(BinarySafeHandle handle)
        {
            using (var inBinary = new Binary(handle))
            {
                var managedBytes = new byte[inBinary.Length];
                Marshal.Copy(inBinary.Data, managedBytes, 0, (int)inBinary.Length); // uint to int, but int should be enough here
                return managedBytes;
            }
        }
    }
}
