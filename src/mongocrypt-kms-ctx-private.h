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

#ifndef MONGOCRYPT_KMX_CTX_PRIVATE_H
#define MONGOCRYPT_KMX_CTX_PRIVATE_H

#include "mongocrypt.h"
#include "mongocrypt-compat.h"
#include "mongocrypt-buffer-private.h"
#include "mongocrypt-cache-key-private.h"
#include "mongocrypt-endpoint-private.h"
#include "mongocrypt-opts-private.h"
#include "kms_message/kms_message.h"
#include "kms_message/kms_kmip_request.h"
#include "mongocrypt-crypto-private.h"

struct __mongocrypt_ctx_opts_t;

typedef enum {
   MONGOCRYPT_KMS_AWS_ENCRYPT,
   MONGOCRYPT_KMS_AWS_DECRYPT,
   MONGOCRYPT_KMS_AZURE_OAUTH,
   MONGOCRYPT_KMS_AZURE_WRAPKEY,
   MONGOCRYPT_KMS_AZURE_UNWRAPKEY,
   MONGOCRYPT_KMS_GCP_OAUTH,
   MONGOCRYPT_KMS_GCP_ENCRYPT,
   MONGOCRYPT_KMS_GCP_DECRYPT,

   MONGOCRYPT_KMS_KMIP_ENCRYPT,
   MONGOCRYPT_KMS_KMIP_MAC,
   MONGOCRYPT_KMS_KMIP_DECRYPT,
   MONGOCRYPT_KMS_KMIP_MAC_VERIFY,
} _kms_request_type_t;

struct _mongocrypt_kms_ctx_t {
   kms_request_t *req;
   _kms_request_type_t req_type;
   kms_response_parser_t *parser;
   mongocrypt_status_t *status;
   _mongocrypt_buffer_t msg;
   _mongocrypt_buffer_t result;
   char *endpoint;
   _mongocrypt_log_t *log;
   // TODO - union with the other parser
   kms_kmip_response_parser_t *kmip_parser;

};


bool
_mongocrypt_kms_ctx_init_aws_decrypt (mongocrypt_kms_ctx_t *kms,
                                      _mongocrypt_opts_t *crypt_opts,
                                      _mongocrypt_key_doc_t *key,
                                      _mongocrypt_log_t *log,
                                      _mongocrypt_crypto_t *crypto)
   MONGOCRYPT_WARN_UNUSED_RESULT;


bool
_mongocrypt_kms_ctx_init_aws_encrypt (
   mongocrypt_kms_ctx_t *kms,
   _mongocrypt_opts_t *crypt_opts,
   struct __mongocrypt_ctx_opts_t *ctx_opts,
   _mongocrypt_buffer_t *decrypted_key_material,
   _mongocrypt_log_t *log,
   _mongocrypt_crypto_t *crypto) MONGOCRYPT_WARN_UNUSED_RESULT;

bool
_mongocrypt_kms_ctx_result (mongocrypt_kms_ctx_t *kms,
                            _mongocrypt_buffer_t *out)
   MONGOCRYPT_WARN_UNUSED_RESULT;

void
_mongocrypt_kms_ctx_cleanup (mongocrypt_kms_ctx_t *kms);

bool
_mongocrypt_kms_ctx_init_azure_auth (mongocrypt_kms_ctx_t *kms,
                                     _mongocrypt_log_t *log,
                                     _mongocrypt_opts_t *crypt_opts,
                                     _mongocrypt_endpoint_t *key_vault_endpoint)
   MONGOCRYPT_WARN_UNUSED_RESULT;

bool
_mongocrypt_kms_ctx_init_azure_wrapkey (
   mongocrypt_kms_ctx_t *kms,
   _mongocrypt_log_t *log,
   _mongocrypt_opts_t *crypt_opts,
   struct __mongocrypt_ctx_opts_t *ctx_opts,
   const char *access_token,
   _mongocrypt_buffer_t *plaintext_key_material) MONGOCRYPT_WARN_UNUSED_RESULT;

bool
_mongocrypt_kms_ctx_init_azure_unwrapkey (mongocrypt_kms_ctx_t *kms,
                                          _mongocrypt_opts_t *crypt_opts,
                                          const char *access_token,
                                          _mongocrypt_key_doc_t *key,
                                          _mongocrypt_log_t *log)
   MONGOCRYPT_WARN_UNUSED_RESULT;

bool
_mongocrypt_kms_ctx_init_gcp_auth (mongocrypt_kms_ctx_t *kms,
                                   _mongocrypt_log_t *log,
                                   _mongocrypt_opts_t *crypt_opts,
                                   _mongocrypt_endpoint_t *kms_endpoint)
   MONGOCRYPT_WARN_UNUSED_RESULT;

bool
_mongocrypt_kms_ctx_init_gcp_encrypt (
   mongocrypt_kms_ctx_t *kms,
   _mongocrypt_log_t *log,
   _mongocrypt_opts_t *crypt_opts,
   struct __mongocrypt_ctx_opts_t *ctx_opts,
   const char *access_token,
   _mongocrypt_buffer_t *plaintext_key_material) MONGOCRYPT_WARN_UNUSED_RESULT;

bool
_mongocrypt_kms_ctx_init_gcp_decrypt (mongocrypt_kms_ctx_t *kms,
                                      _mongocrypt_opts_t *crypt_opts,
                                      const char *access_token,
                                      _mongocrypt_key_doc_t *key,
                                      _mongocrypt_log_t *log)
   MONGOCRYPT_WARN_UNUSED_RESULT;


bool
_mongocrypt_kms_ctx_init_kmip_encrypt (   mongocrypt_kms_ctx_t *kms,
_mongocrypt_crypto_t* crypto,
   _mongocrypt_log_t *log,
   _mongocrypt_opts_t *crypt_opts,
   struct __mongocrypt_ctx_opts_t *ctx_opts,
   _mongocrypt_buffer_t *plaintext_key_material);


bool
_mongocrypt_kms_ctx_init_kmip_mac (
   mongocrypt_kms_ctx_t *kms,
   _mongocrypt_log_t *log,
   _mongocrypt_opts_t *crypt_opts,
   struct __mongocrypt_ctx_opts_t *ctx_opts,
   _mongocrypt_buffer_t *plaintext_key_material);


bool
_mongocrypt_kms_ctx_init_kmip_mac_verify (mongocrypt_kms_ctx_t *kms,
                                      _mongocrypt_opts_t *crypt_opts,
                                      _mongocrypt_key_doc_t *key,
                                      _mongocrypt_log_t *log);

bool
_mongocrypt_kms_ctx_init_kmip_decrypt (mongocrypt_kms_ctx_t *kms,
                                      _mongocrypt_opts_t *crypt_opts,
                                      _mongocrypt_key_doc_t *key,
                                      _mongocrypt_log_t *log);

#endif /* MONGOCRYPT_KMX_CTX_PRIVATE_H */
