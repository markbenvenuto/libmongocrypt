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

    public class CryptContext
    {

    }

    public class Library
    {
        static Library()
        {
            LibraryLoader loader = new LibraryLoader();

            mongocrypt_version = loader.GetFunction<Delegates.mongocrypt_version>("mongocrypt_version");

            mongocrypt_new = loader.GetFunction<Delegates.mongocrypt_new>("mongocrypt_new");
            mongocrypt_destroy = loader.GetFunction<Delegates.mongocrypt_destroy>("mongocrypt_destroy");

            mongocrypt_opts_new = loader.GetFunction<Delegates.mongocrypt_opts_new>("mongocrypt_opts_new");
            mongocrypt_opts_destroy = loader.GetFunction<Delegates.mongocrypt_opts_destroy>("mongocrypt_opts_destroy");
            mongocrypt_opts_set_opt = loader.GetFunction<Delegates.mongocrypt_opts_set_opt>("mongocrypt_opts_set_opt");

            mongocrypt_status_new = loader.GetFunction<Delegates.mongocrypt_status_new>("mongocrypt_status_new");
            mongocrypt_status_destroy = loader.GetFunction<Delegates.mongocrypt_status_destroy>("mongocrypt_status_destroy");
            mongocrypt_status_type = loader.GetFunction<Delegates.mongocrypt_status_type>("mongocrypt_status_type");
            mongocrypt_status_code = loader.GetFunction<Delegates.mongocrypt_status_code>("mongocrypt_status_code");
            mongocrypt_status_message = loader.GetFunction<Delegates.mongocrypt_status_message>("mongocrypt_status_message");
            mongocrypt_status_ok = loader.GetFunction<Delegates.mongocrypt_status_ok>("mongocrypt_status_ok");

            mongocrypt_binary_new = loader.GetFunction<Delegates.mongocrypt_binary_new>("mongocrypt_binary_new");
            mongocrypt_binary_destroy = loader.GetFunction<Delegates.mongocrypt_binary_destroy>("mongocrypt_binary_destroy");
            mongocrypt_binary_new_from_data = loader.GetFunction<Delegates.mongocrypt_binary_new_from_data>("mongocrypt_binary_new_from_data");
            mongocrypt_binary_data = loader.GetFunction<Delegates.mongocrypt_binary_data>("mongocrypt_binary_data");
            mongocrypt_binary_len = loader.GetFunction<Delegates.mongocrypt_binary_len>("mongocrypt_binary_len");

            mongocrypt_ctx_new = loader.GetFunction<Delegates.mongocrypt_ctx_new>("mongocrypt_ctx_new");
            mongocrypt_ctx_status = loader.GetFunction<Delegates.mongocrypt_ctx_status>("mongocrypt_ctx_status");
            mongocrypt_ctx_encrypt_init = loader.GetFunction<Delegates.mongocrypt_ctx_encrypt_init>("mongocrypt_ctx_encrypt_init");
            mongocrypt_ctx_decrypt_init = loader.GetFunction<Delegates.mongocrypt_ctx_decrypt_init>("mongocrypt_ctx_decrypt_init");
            mongocrypt_ctx_state = loader.GetFunction<Delegates.mongocrypt_ctx_state>("mongocrypt_ctx_state");
            mongocrypt_ctx_mongo_op = loader.GetFunction<Delegates.mongocrypt_ctx_mongo_op>("mongocrypt_ctx_mongo_op");
            mongocrypt_ctx_mongo_feed = loader.GetFunction<Delegates.mongocrypt_ctx_mongo_feed>("mongocrypt_ctx_mongo_feed");
            mongocrypt_ctx_mongo_done = loader.GetFunction<Delegates.mongocrypt_ctx_mongo_done>("mongocrypt_ctx_mongo_done");
            mongocrypt_ctx_next_kms_ctx = loader.GetFunction<Delegates.mongocrypt_ctx_next_kms_ctx>("mongocrypt_ctx_next_kms_ctx");
            mongocrypt_kms_ctx_message = loader.GetFunction<Delegates.mongocrypt_kms_ctx_message>("mongocrypt_kms_ctx_message");
            mongocrypt_kms_ctx_bytes_needed = loader.GetFunction<Delegates.mongocrypt_kms_ctx_bytes_needed>("mongocrypt_kms_ctx_bytes_needed");
            mongocrypt_kms_ctx_feed = loader.GetFunction<Delegates.mongocrypt_kms_ctx_feed>("mongocrypt_kms_ctx_feed");
            mongocrypt_kms_ctx_status = loader.GetFunction<Delegates.mongocrypt_kms_ctx_status>("mongocrypt_kms_ctx_status");
            mongocrypt_ctx_kms_done = loader.GetFunction<Delegates.mongocrypt_ctx_kms_done>("mongocrypt_ctx_kms_done");
            mongocrypt_ctx_finalize = loader.GetFunction<Delegates.mongocrypt_ctx_finalize>("mongocrypt_ctx_finalize");
            mongocrypt_ctx_destroy = loader.GetFunction<Delegates.mongocrypt_ctx_destroy>("mongocrypt_ctx_destroy");

        }

        public static string Version
        {
            get
            {
                IntPtr p = mongocrypt_version();
                return Marshal.PtrToStringAnsi(p);
            }
        }

        internal static readonly Delegates.mongocrypt_version mongocrypt_version;

        internal static readonly Delegates.mongocrypt_new mongocrypt_new;
        internal static readonly Delegates.mongocrypt_destroy mongocrypt_destroy;

        internal static readonly Delegates.mongocrypt_opts_new mongocrypt_opts_new;
        internal static readonly Delegates.mongocrypt_opts_destroy mongocrypt_opts_destroy;
        internal static readonly Delegates.mongocrypt_opts_set_opt mongocrypt_opts_set_opt;

        internal static readonly Delegates.mongocrypt_status_new mongocrypt_status_new;
        internal static readonly Delegates.mongocrypt_status_destroy mongocrypt_status_destroy;

        internal static readonly Delegates.mongocrypt_status_type mongocrypt_status_type;
        internal static readonly Delegates.mongocrypt_status_code mongocrypt_status_code;
        internal static readonly Delegates.mongocrypt_status_message mongocrypt_status_message;
        internal static readonly Delegates.mongocrypt_status_ok mongocrypt_status_ok;

        internal static readonly Delegates.mongocrypt_binary_new mongocrypt_binary_new;
        internal static readonly Delegates.mongocrypt_binary_destroy mongocrypt_binary_destroy;
        internal static readonly Delegates.mongocrypt_binary_new_from_data mongocrypt_binary_new_from_data;
        internal static readonly Delegates.mongocrypt_binary_data mongocrypt_binary_data;
        internal static readonly Delegates.mongocrypt_binary_len mongocrypt_binary_len;


        internal static readonly Delegates.mongocrypt_ctx_new mongocrypt_ctx_new;
        internal static readonly Delegates.mongocrypt_ctx_status mongocrypt_ctx_status;
        internal static readonly Delegates.mongocrypt_ctx_encrypt_init mongocrypt_ctx_encrypt_init;
        internal static readonly Delegates.mongocrypt_ctx_decrypt_init mongocrypt_ctx_decrypt_init;
        internal static readonly Delegates.mongocrypt_ctx_state mongocrypt_ctx_state;
        internal static readonly Delegates.mongocrypt_ctx_mongo_op mongocrypt_ctx_mongo_op;
        internal static readonly Delegates.mongocrypt_ctx_mongo_feed mongocrypt_ctx_mongo_feed;
        internal static readonly Delegates.mongocrypt_ctx_mongo_done mongocrypt_ctx_mongo_done;
        internal static readonly Delegates.mongocrypt_ctx_next_kms_ctx mongocrypt_ctx_next_kms_ctx;
        internal static readonly Delegates.mongocrypt_kms_ctx_message mongocrypt_kms_ctx_message;
        internal static readonly Delegates.mongocrypt_kms_ctx_bytes_needed mongocrypt_kms_ctx_bytes_needed;
        internal static readonly Delegates.mongocrypt_kms_ctx_feed mongocrypt_kms_ctx_feed;
        internal static readonly Delegates.mongocrypt_kms_ctx_status mongocrypt_kms_ctx_status;
        internal static readonly Delegates.mongocrypt_ctx_kms_done mongocrypt_ctx_kms_done;
        internal static readonly Delegates.mongocrypt_ctx_finalize mongocrypt_ctx_finalize;
        internal static readonly Delegates.mongocrypt_ctx_destroy mongocrypt_ctx_destroy;

        internal enum Options
        {
            MONGOCRYPT_AWS_REGION,
            MONGOCRYPT_AWS_SECRET_ACCESS_KEY,
            MONGOCRYPT_AWS_ACCESS_KEY_ID,
            MONGOCRYPT_LOG_FN,
            MONGOCRYPT_LOG_CTX
        }

        internal enum ErrorType
        {
            MONGOCRYPT_type_NONE = 0,
            MONGOCRYPT_type_MONGOCRYPTD,
            MONGOCRYPT_type_KMS,
            MONGOCRYPT_type_CLIENT
        }

internal  enum ContextState {
   MONGOCRYPT_CTX_ERROR = 0,
   MONGOCRYPT_CTX_NOTHING_TO_DO = 1,
   MONGOCRYPT_CTX_NEED_MONGO_COLLINFO = 2, /* run on main MongoClient */
   MONGOCRYPT_CTX_NEED_MONGO_MARKINGS = 3, /* run on mongocryptd. */
   MONGOCRYPT_CTX_NEED_MONGO_KEYS = 4,     /* run on key vault */
   MONGOCRYPT_CTX_NEED_KMS = 5,
   MONGOCRYPT_CTX_READY = 6, /* ready for encryption/decryption */
   MONGOCRYPT_CTX_DONE = 7
};

        internal class Delegates
        {
            public delegate IntPtr mongocrypt_version();

            public delegate MongoCryptSafeHandle mongocrypt_new(OptionsSafeHandle ptr);
            public delegate void mongocrypt_destroy(IntPtr ptr);

            public delegate OptionsSafeHandle mongocrypt_opts_new();
            public delegate void mongocrypt_opts_destroy(IntPtr ptr);
            public delegate void mongocrypt_opts_set_opt(OptionsSafeHandle ptr, Options opts, IntPtr value);

            public delegate StatusSafeHandle mongocrypt_status_new();
            public delegate void mongocrypt_status_destroy(IntPtr ptr);
            public delegate ErrorType mongocrypt_status_type(StatusSafeHandle ptr);
            public delegate UInt32 mongocrypt_status_code(StatusSafeHandle ptr);
            public delegate IntPtr mongocrypt_status_message(StatusSafeHandle ptr);
            public delegate bool mongocrypt_status_ok(StatusSafeHandle ptr);

            public delegate BinarySafeHandle mongocrypt_binary_new();
            public delegate void mongocrypt_binary_destroy(IntPtr ptr);
            public delegate void mongocrypt_binary_new_from_data(IntPtr ptr, UInt32 len);
            public delegate void mongocrypt_binary_data(BinarySafeHandle handle);
            public delegate void mongocrypt_binary_len(BinarySafeHandle handle);

            public delegate ContextSafeHandle mongocrypt_ctx_new(MongoCryptSafeHandle handle);
            public delegate bool mongocrypt_ctx_status(MongoCryptSafeHandle handle, StatusSafeHandle status);
            public delegate bool mongocrypt_ctx_encrypt_init(MongoCryptSafeHandle handle, IntPtr ns, UInt32 len);
            public delegate bool mongocrypt_ctx_decrypt_init(MongoCryptSafeHandle handle, BinarySafeHandle binary);
            public delegate ContextState mongocrypt_ctx_state(MongoCryptSafeHandle handle);
            public delegate bool mongocrypt_ctx_mongo_op(MongoCryptSafeHandle handle, BinarySafeHandle bsonOp);
            public delegate bool mongocrypt_ctx_mongo_feed(MongoCryptSafeHandle handle, BinarySafeHandle reply);
            public delegate bool mongocrypt_ctx_mongo_done(MongoCryptSafeHandle handle);

            public delegate IntPtr mongocrypt_ctx_next_kms_ctx(MongoCryptSafeHandle handle);
            public delegate bool mongocrypt_kms_ctx_message(IntPtr handle, BinarySafeHandle binary);
            public delegate UInt32 mongocrypt_kms_ctx_bytes_needed(IntPtr handle);
            public delegate bool mongocrypt_kms_ctx_feed(IntPtr handle, BinarySafeHandle binary);
            public delegate bool mongocrypt_kms_ctx_status(IntPtr handle, StatusSafeHandle status);
            public delegate bool mongocrypt_ctx_kms_done(MongoCryptSafeHandle handle);

            public delegate bool mongocrypt_ctx_finalize(MongoCryptSafeHandle handle, BinarySafeHandle binary);
            public delegate void mongocrypt_ctx_destroy(IntPtr ptr);
        }
    }
}