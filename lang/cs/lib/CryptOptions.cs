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

    public class CryptOptions : IDisposable
    {
        public CryptOptions() {
            _handle = Library.mongocrypt_opts_new();
        }

        public string AwsRegion
        {
            set
            {
                SetOption(Library.Options.MONGOCRYPT_AWS_REGION, value);
            }
        }

        public string AwsSecretAccessKey
        {
            set
            {
                SetOption(Library.Options.MONGOCRYPT_AWS_SECRET_ACCESS_KEY, value);
            }
        }

        public string AwsAccessKeyId
        {
            set
            {
                SetOption(Library.Options.MONGOCRYPT_AWS_ACCESS_KEY_ID, value);
            }
        }


        // TODO: - add configurable logging support

        internal OptionsSafeHandle Handle => _handle;

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


        private void SetOption(Library.Options option, string value)
        {
            IntPtr stringPointer = (IntPtr)Marshal.StringToHGlobalAnsi(value);

            Library.mongocrypt_opts_set_opt(_handle, option, stringPointer);

            Marshal.FreeHGlobal(stringPointer);
        }

        private OptionsSafeHandle _handle;
    }
}