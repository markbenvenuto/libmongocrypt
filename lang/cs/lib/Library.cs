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
            mongocrypt_status_error_type = loader.GetFunction<Delegates.mongocrypt_status_error_type>("mongocrypt_status_error_type");
            mongocrypt_status_code = loader.GetFunction<Delegates.mongocrypt_status_code>("mongocrypt_status_code");
            mongocrypt_status_message = loader.GetFunction<Delegates.mongocrypt_status_message>("mongocrypt_status_message");
            mongocrypt_status_ok = loader.GetFunction<Delegates.mongocrypt_status_ok>("mongocrypt_status_ok");

        }

        public static string Version
        {
            get {
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

        internal static readonly Delegates.mongocrypt_status_error_type mongocrypt_status_error_type;
        internal static readonly Delegates.mongocrypt_status_code mongocrypt_status_code;
        internal static readonly Delegates.mongocrypt_status_message mongocrypt_status_message;
        internal static readonly Delegates.mongocrypt_status_ok mongocrypt_status_ok;

        internal enum Options
        {
            MONGOCRYPT_AWS_REGION,
            MONGOCRYPT_AWS_SECRET_ACCESS_KEY,
            MONGOCRYPT_AWS_ACCESS_KEY_ID,
            MONGOCRYPT_LOG_FN,
            MONGOCRYPT_LOG_CTX
        }

        internal enum ErrorType {
            MONGOCRYPT_ERROR_TYPE_NONE = 0,
            MONGOCRYPT_ERROR_TYPE_MONGOCRYPTD,
            MONGOCRYPT_ERROR_TYPE_KMS,
            MONGOCRYPT_ERROR_TYPE_CLIENT
        }

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
            public delegate ErrorType mongocrypt_status_error_type(StatusSafeHandle ptr);
            public delegate Int32 mongocrypt_status_code(StatusSafeHandle ptr);
            public delegate IntPtr mongocrypt_status_message(StatusSafeHandle ptr);
            public delegate bool mongocrypt_status_ok(StatusSafeHandle ptr);

        }
    }
}