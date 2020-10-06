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

#include "kms_crypto.h"

#ifdef KMS_MESSAGE_ENABLE_CRYPTO_CNG

// tell windows.h not to include a bunch of headers we don't need:
#define WIN32_LEAN_AND_MEAN

// Tell windows.h not to define any NT status codes, so that we can
// get the definitions from ntstatus.h, which has a more complete list.
#define WIN32_NO_STATUS

#include <windows.h>

#undef WIN32_NO_STATUS

// Obtain a definition for the ntstatus type.
#include <winternl.h>

// Add back in the status definitions so that macro expansions for
// things like STILL_ACTIVE and WAIT_OBJECT_O can be resolved (they
// expand to STATUS_ codes).
#include <ntstatus.h>

#include <bcrypt.h>
#include <wincrypt.h>

static BCRYPT_ALG_HANDLE _algoSHA256 = 0;
static BCRYPT_ALG_HANDLE _algoSHA256Hmac = 0;

int
kms_crypto_init ()
{
   if (BCryptOpenAlgorithmProvider (
          &_algoSHA256, BCRYPT_SHA256_ALGORITHM, MS_PRIMITIVE_PROVIDER, 0) !=
       STATUS_SUCCESS) {
      return 1;
   }

   if (BCryptOpenAlgorithmProvider (&_algoSHA256Hmac,
                                    BCRYPT_SHA256_ALGORITHM,
                                    MS_PRIMITIVE_PROVIDER,
                                    BCRYPT_ALG_HANDLE_HMAC_FLAG) !=
       STATUS_SUCCESS) {
      return 2;
   }

   return 0;
}

void
kms_crypto_cleanup ()
{
   (void) BCryptCloseAlgorithmProvider (_algoSHA256, 0);
   (void) BCryptCloseAlgorithmProvider (_algoSHA256Hmac, 0);
}

bool
kms_sha256 (void *unused_ctx,
            const char *input,
            size_t len,
            unsigned char *hash_out)
{
   BCRYPT_HASH_HANDLE hHash;

   NTSTATUS status =
      BCryptCreateHash (_algoSHA256, &hHash, NULL, 0, NULL, 0, 0);
   if (status != STATUS_SUCCESS) {
      return 0;
   }

   status = BCryptHashData (hHash, (PUCHAR) (input), (ULONG) len, 0);
   if (status != STATUS_SUCCESS) {
      goto cleanup;
   }

   // Hardcode output length
   status = BCryptFinishHash (hHash, hash_out, 256 / 8, 0);
   if (status != STATUS_SUCCESS) {
      goto cleanup;
   }

cleanup:
   (void) BCryptDestroyHash (hHash);

   return status == STATUS_SUCCESS ? 1 : 0;
}

bool
kms_sha256_hmac (void *unused_ctx,
                 const char *key_input,
                 size_t key_len,
                 const char *input,
                 size_t len,
                 unsigned char *hash_out)
{
   BCRYPT_HASH_HANDLE hHash;

   NTSTATUS status = BCryptCreateHash (
      _algoSHA256Hmac, &hHash, NULL, 0, (PUCHAR) key_input, (ULONG) key_len, 0);
   if (status != STATUS_SUCCESS) {
      return 0;
   }

   status = BCryptHashData (hHash, (PUCHAR) input, (ULONG) len, 0);
   if (status != STATUS_SUCCESS) {
      goto cleanup;
   }

   // Hardcode output length
   status = BCryptFinishHash (hHash, hash_out, 256 / 8, 0);
   if (status != STATUS_SUCCESS) {
      goto cleanup;
   }

cleanup:
   (void) BCryptDestroyHash (hHash);

   return status == STATUS_SUCCESS ? 1 : 0;
}

bool
kms_sign_rsaes_pkcs1_v1_5 (void *unused_ctx,
                           const char *private_key,
                           size_t private_key_len,
                           const char *input,
                           size_t input_len,
                           unsigned char *signature_out)
{
   HCRYPTPROV provider = 0;
   PCCERT_CONTEXT cert = NULL;
   LPBYTE blob_private = NULL;
   DWORD blob_private_len = 0;
   LPBYTE raw_private = NULL;
   DWORD raw_private_len = 0;
   //DWORD encrypted_private_len = 0;
   //LPBYTE encrypted_private = NULL;
   HCRYPTHASH hHash = 0;
   HCRYPTKEY hKey = 0;
   bool success;
   bool ret = false;
   DWORD dwSigLen = 256;

   ///* https://msdn.microsoft.com/en-us/library/windows/desktop/aa380285%28v=vs.85%29.aspx
   // */
   //success =
   //   CryptStringToBinaryA (private_key,               /* pszString */
   //                         (DWORD) private_key_len,           /* cchString */
   //                         CRYPT_STRING_BASE64HEADER, /* dwFlags */
   //                         NULL,                      /* pbBinary */
   //                         &encrypted_private_len,    /* pcBinary, IN/OUT */
   //                         NULL,                      /* pdwSkip */
   //                         NULL);                     /* pdwFlags */
   //if (!success) {
   //   // MONGOC_ERROR ("Failed to convert base64 private key. Error 0x%.8X",
   //   //               (unsigned int) GetLastError ());
   //   goto fail;
   //}

   //encrypted_private = (LPBYTE) calloc (1, encrypted_private_len);
   //success = CryptStringToBinaryA (private_key,
   //                                (DWORD) private_key_len,
   //                                CRYPT_STRING_BASE64HEADER,
   //                                encrypted_private,
   //                                &encrypted_private_len,
   //                                NULL,
   //                                NULL);
   //if (!success) {
   //   // MONGOC_ERROR ("Failed to convert base64 private key. Error 0x%.8X",
   //   //               (unsigned int) GetLastError ());
   //   goto fail;
   //}

   /* https://msdn.microsoft.com/en-us/library/windows/desktop/aa379912%28v=vs.85%29.aspx
    */
   success = CryptDecodeObjectEx (
      X509_ASN_ENCODING, /* dwCertEncodingType */
                                  PKCS_PRIVATE_KEY_INFO,   /* lpszStructType */
      private_key,                             /* pbEncoded */
      (DWORD)private_key_len,                         /* cbEncoded */
      0,                                       /* dwFlags */
      NULL,                                    /* pDecodePara */
      NULL,                                    /* pvStructInfo */
      &blob_private_len);                      /* pcbStructInfo */
   if (!success) {
      // LPTSTR msg = NULL;
      // FormatMessage (FORMAT_MESSAGE_ALLOCATE_BUFFER |
      //                   FORMAT_MESSAGE_FROM_SYSTEM |
      //                   FORMAT_MESSAGE_ARGUMENT_ARRAY,
      //                NULL,
      //                GetLastError (),
      //                LANG_NEUTRAL,
      //                (LPTSTR) &msg,
      //                0,
      //                NULL);
      // MONGOC_ERROR ("Failed to parse private key. %s (0x%.8X)",
      //               msg,
      //               (unsigned int) GetLastError ());
      // LocalFree (msg);
      int gle = GetLastError ();

      goto fail;
   }

   blob_private = (LPBYTE) calloc (1, blob_private_len);
   success = CryptDecodeObjectEx (X509_ASN_ENCODING ,
                                  PKCS_PRIVATE_KEY_INFO,
                                  private_key,
                                  (DWORD)private_key_len,
                                  0,
                                  NULL,
                                  blob_private,
                                  &blob_private_len);
   if (!success) {
      //    MONGOC_ERROR ("Failed to parse private key. Error 0x%.8X",
      //                  (unsigned int) GetLastError ());
      int gle = GetLastError ();
      
      goto fail;
   }

   CRYPT_PRIVATE_KEY_INFO* privateKeyInfo =
      (CRYPT_PRIVATE_KEY_INFO*) blob_private;
         

   success = CryptDecodeObjectEx (X509_ASN_ENCODING, /* dwCertEncodingType */
                                  PKCS_RSA_PRIVATE_KEY, /* lpszStructType */
                           privateKeyInfo->PrivateKey.pbData, /* pbEncoded */
      (DWORD) privateKeyInfo->PrivateKey.cbData, /* cbEncoded */
                                  0,                       /* dwFlags */
                                  NULL,                    /* pDecodePara */
                                  NULL,                    /* pvStructInfo */
                                  &raw_private_len);      /* pcbStructInfo */
   if (!success) {
      // LPTSTR msg = NULL;
      // FormatMessage (FORMAT_MESSAGE_ALLOCATE_BUFFER |
      //                   FORMAT_MESSAGE_FROM_SYSTEM |
      //                   FORMAT_MESSAGE_ARGUMENT_ARRAY,
      //                NULL,
      //                GetLastError (),
      //                LANG_NEUTRAL,
      //                (LPTSTR) &msg,
      //                0,
      //                NULL);
      // MONGOC_ERROR ("Failed to parse private key. %s (0x%.8X)",
      //               msg,
      //               (unsigned int) GetLastError ());
      // LocalFree (msg);
      int gle = GetLastError ();

      goto fail;
   }

   raw_private = (LPBYTE) calloc (1, raw_private_len);
   success = CryptDecodeObjectEx (X509_ASN_ENCODING,
                                  PKCS_RSA_PRIVATE_KEY,
                                  privateKeyInfo->PrivateKey.pbData,
                                  (DWORD) privateKeyInfo->PrivateKey.cbData,
                                  0,
                                  NULL,
                                  raw_private,
                                  &raw_private_len);
   if (!success) {
      //    MONGOC_ERROR ("Failed to parse private key. Error 0x%.8X",
      //                  (unsigned int) GetLastError ());
      int gle = GetLastError ();

      goto fail;
   }

   /* https://msdn.microsoft.com/en-us/library/windows/desktop/aa379886%28v=vs.85%29.aspx
    */
   success = CryptAcquireContext (&provider,            /* phProv */
                                  NULL,                 /* pszContainer */
                                  MS_ENH_RSA_AES_PROV, /* pszProvider */
                                  PROV_RSA_AES,        /* dwProvType */
                           CRYPT_VERIFYCONTEXT | CRYPT_SILENT); /* dwFlags */
   if (!success) {
      // MONGOC_ERROR ("CryptAcquireContext failed with error 0x%.8X",
      //               (unsigned int) GetLastError ());
      int gle = GetLastError ();
      
      goto fail;
   }

   /* https://msdn.microsoft.com/en-us/library/windows/desktop/aa380207%28v=vs.85%29.aspx
    */
   success = CryptImportKey (provider,         /* hProv */
                             raw_private,      /* pbData */
                             raw_private_len,  /* dwDataLen */
                             0,                /* hPubKey */
                             CRYPT_EXPORTABLE, /* dwFlags */
                             &hKey);           /* phKey, OUT */
   if (!success) {
      // MONGOC_ERROR ("CryptImportKey for private key failed with error
      // 0x%.8X",
      //               (unsigned int) GetLastError ());
      
             int gle = GetLastError ();
      goto fail;
   }


    NTSTATUS Status;
   BCRYPT_KEY_HANDLE KeyHandle = NULL;
   BCRYPT_ALG_HANDLE DsaAlgHandle = NULL;


      //
   // Open a DSA algorithm handle
   //

   Status = BCryptOpenAlgorithmProvider (
      &DsaAlgHandle, BCRYPT_RSA_ALGORITHM, NULL, 0);
   if (!NT_SUCCESS (Status)) {
         int gle = GetLastError ();
      goto fail;
   }

   //
   // Import the public key
   //

   Status = BCryptImportKeyPair (
      DsaAlgHandle,           // Alg handle
      NULL,                   // Parameter not used
      LEGACY_RSAPRIVATE_BLOB, // Blob type (Null terminated unicode string)
      &KeyHandle,             // Key handle that will be recieved
      raw_private,            // Buffer than points to the key blob
      raw_private_len,        // Buffer length in bytes
      0);                     // Flags
   if (!NT_SUCCESS (Status)) {
         int gle = GetLastError ();
      goto fail;
   }


   //    Status = BCryptOpenAlgorithmProvider (
   //   &DsaAlgHandle, BCRYPT_RSA_ALGORITHM, NULL, 0);
   //if (!NT_SUCCESS (Status)) {
   //       goto fail;
   //}

   ////
   //// Generate a 1024 bit DSA key
   ////

   //Status = BCryptGenerateKeyPair (DsaAlgHandle, &KeyHandle, 2048, 0);
   //if (!NT_SUCCESS (Status)) {
   //   goto fail;
   //}

   ////
   //// Finalize the key
   ////

   //Status = BCryptFinalizeKeyPair (KeyHandle, 0);
   //if (!NT_SUCCESS (Status)) {
   //   goto fail;
   //}


//
//   if (!CryptCreateHash (provider, CALG_SHA_256, 0, 0, &hHash)) {
//      int gle = GetLastError ();
//      
//      goto fail;
//   }
//
//   if (!CryptHashData (hHash, input, (DWORD)input_len, 0)) {
//      int gle = GetLastError ();
//
//      goto fail;
//   }
//
//
//DWORD foo1 = 0; 
//      if (!CryptSignHash (hHash,
//                       AT_KEYEXCHANGE,
//                       NULL,
//                       0,
//                       NULL, &foo1)) {
//      int gle = GetLastError ();
//
//      goto fail;
//   }
//
//
//
//   if (!CryptSignHash (hHash,
//                       AT_KEYEXCHANGE,
//                       NULL,
//                       0,
//                       signature_out,
//                       &dwSigLen)) {
//      int gle = GetLastError ();
//      
//      goto fail;
//   }

   char *hash1 = malloc (32);
   kms_sha256 (NULL, input, input_len, hash1);

   BCRYPT_PKCS1_PADDING_INFO padding_PKCS1;
   padding_PKCS1.pszAlgId = BCRYPT_SHA256_ALGORITHM;

   DWORD SignatureBlobLength = 0;
   DWORD c1 = 256;
   Status = BCryptSignHash (
      KeyHandle,            // Key handle used to sign the hash
      NULL,                 // Padding information
      hash1,        // Hash of the message
      32,  // Length of the hash
      NULL,                 // Signed hash buffer
      0,                    // Length of the signature(signed hash value)
      &SignatureBlobLength, // Number of bytes copied to the signature buffer
      0);                   // Flags
   if (!NT_SUCCESS (Status)) {
      int gle = GetLastError ();
      goto fail;
   }
    

   memset (signature_out, 0, 256);
   Status = BCryptSignHash (
      KeyHandle,           // Key handle used to sign the hash
                      &padding_PKCS1,
                                    // Padding information
                      hash1,               // Hash of the message
      32, // Length of the hash
      signature_out,       // Signed hash buffer
      256, // Length of the signature(signed hash value)
      &c1,       // Number of bytes copied to the signature buffer
                      BCRYPT_PAD_PKCS1); // Flags
   if (!NT_SUCCESS (Status)) {
      int gle = GetLastError ();
      goto fail;
   } 


   ret = true;

fail:
   if (hHash) {
      CryptDestroyHash (hHash);
   }

   if (provider) {
      CryptReleaseContext (provider, 0);
   }

   //free (encrypted_private);
   free (blob_private);
   free (raw_private);

   return ret;
}

#endif /* KMS_MESSAGE_ENABLE_CRYPTO_CNG */
