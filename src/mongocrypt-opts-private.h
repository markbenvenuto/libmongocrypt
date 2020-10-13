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

#ifndef MONGOCRYPT_OPTS_PRIVATE_H
#define MONGOCRYPT_OPTS_PRIVATE_H

#include <bson/bson.h>

#include "mongocrypt.h"
#include "mongocrypt-buffer-private.h"
#include "mongocrypt-log-private.h"
#include "mongocrypt-endpoint-private.h"
#include "mongocrypt-kek-private.h"

typedef struct {
   char *tenant_id;
   char *client_id;
   char *client_secret;
   _mongocrypt_endpoint_t *identity_platform_endpoint;
} _mongocrypt_opts_kms_provider_azure_t;

typedef struct {
   char *email;
   _mongocrypt_buffer_t private_key;
   _mongocrypt_endpoint_t *endpoint;
} _mongocrypt_opts_kms_provider_gcp_t;

typedef struct {
   char *secret_access_key;
   char *access_key_id;
} _mongocrypt_opts_kms_provider_aws_t;

typedef struct {
   _mongocrypt_buffer_t key;
} _mongocrypt_opts_kms_provider_local_t;

typedef struct {
   mongocrypt_log_fn_t log_fn;
   void *log_ctx;
   _mongocrypt_buffer_t schema_map;
   
   int kms_providers; /* A bit set of _mongocrypt_kms_provider_t */
   _mongocrypt_opts_kms_provider_local_t kms_provider_local;
   _mongocrypt_opts_kms_provider_aws_t kms_provider_aws;
   _mongocrypt_opts_kms_provider_azure_t kms_provider_azure;
   _mongocrypt_opts_kms_provider_gcp_t kms_provider_gcp;
   mongocrypt_hmac_fn sign_rsaes_pkcs1_v1_5;
   void *sign_ctx;
} _mongocrypt_opts_t;


void
_mongocrypt_opts_init (_mongocrypt_opts_t *opts);


void
_mongocrypt_opts_cleanup (_mongocrypt_opts_t *opts);


bool
_mongocrypt_opts_validate (_mongocrypt_opts_t *opts,
                           mongocrypt_status_t *status)
   MONGOCRYPT_WARN_UNUSED_RESULT;

/*
 * Parse an optional UTF-8 value from BSON.
 * @dotkey may be a dot separated key like: "a.b.c".
 * @*out is set to a copy of the string if found, NULL otherwise. Caller must
 * clean up with bson_free (*out).
 * Returns true if no error occured.
 */
bool
_mongocrypt_parse_optional_utf8 (const bson_t *bson,
                                 const char *dotkey,
                                 char **out,
                                 mongocrypt_status_t *status);

/*
 * Parse a required UTF-8 value from BSON.
 * @dotkey may be a dot separated key like: "a.b.c".
 * @*out is set to a copy of the string if found, NULL otherwise. Caller must
 * clean up with bson_free (*out).
 * Returns true if no error occured.
 */
bool
_mongocrypt_parse_required_utf8 (const bson_t *bson,
                                 const char *dotkey,
                                 char **out,
                                 mongocrypt_status_t *status);

/*
 * Parse an optional endpoint UTF-8 from BSON.
 * @dotkey may be a dot separated key like: "a.b.c".
 * @*out is set to a new _mongocrypt_endpoint_t of the if found, NULL otherwise.
 * Caller must clean up with _mongocrypt_endpoint_destroy (*out).
 * Returns true if no error occured.
 */
bool
_mongocrypt_parse_optional_endpoint (const bson_t *bson,
                                     const char *dotkey,
                                     _mongocrypt_endpoint_t **out,
                                     mongocrypt_status_t *status);

/*
 * Parse a required endpoint UTF-8 from BSON.
 * @dotkey may be a dot separated key like: "a.b.c".
 * @*out is set to a new _mongocrypt_endpoint_t of the if found, NULL otherwise.
 * Caller must clean up with _mongocrypt_endpoint_destroy (*out).
 * Returns true if no error occured.
 */
bool
_mongocrypt_parse_required_endpoint (const bson_t *bson,
                                     const char *dotkey,
                                     _mongocrypt_endpoint_t **out,
                                     mongocrypt_status_t *status);

/*
 * Parse an optional binary type from BSON.
 * The field parsed is accepted as:
 * - A BSON binary value (of any subtype).
 * - A BSON UTF-8 value, set to base64 encoded data.
 *
 * @dotkey may be a dot separated key like: "a.b.c"
 * @out is initialized with the parsed data, or initialized to empty on error.
 * Caller must clean up with _mongocrypt_buffer_cleanup (out).
 * Returns true if no error occurred.
 */
bool
_mongocrypt_parse_optional_binary (const bson_t *bson,
                                   const char *dotkey,
                                   _mongocrypt_buffer_t *out,
                                   mongocrypt_status_t *status);

/*
 * Parse a required binary type from BSON.
 * The field parsed is accepted as:
 * - A BSON binary value (of any subtype).
 * - A BSON UTF-8 value, set to base64 encoded data.
 *
 * @dotkey may be a dot separated key like: "a.b.c"
 * @out is initialized with the parsed data, or initialized to empty on error.
 * Caller must clean up with _mongocrypt_buffer_cleanup (out).
 * Returns true if no error occurred.
 */
bool
_mongocrypt_parse_required_binary (const bson_t *bson,
                                   const char *dotkey,
                                   _mongocrypt_buffer_t *out,
                                   mongocrypt_status_t *status);

#endif /* MONGOCRYPT_OPTS_PRIVATE_H */
