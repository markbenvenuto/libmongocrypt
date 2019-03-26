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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Security;
using System.Threading;
//using System.Runtime.ConstrainedExecution;


//using System.Security.Permissions;

namespace MongoDB.Crypt
{

    public class Status : IDisposable
    {
        public Status()
        {
            _handle = Library.mongocrypt_status_new();
        }

        void throwExceptionIfNeeded()
        {
            if (!Library.mongocrypt_status_ok(_handle))
            {
                var errorType = Library.mongocrypt_status_error_type(_handle);
                var statusCode = Library.mongocrypt_status_code(_handle);

                IntPtr msgPtr = Library.mongocrypt_status_message(_handle);
                var message = Marshal.PtrToStringAnsi(msgPtr);

                throw new CryptException(errorType, statusCode, message);
            }
        }

    internal StatusSafeHandle Handle => _handle;

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

        private StatusSafeHandle _handle;
    }
}