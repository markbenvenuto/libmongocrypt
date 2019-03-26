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

namespace MongoDB.Crypt
{

    public class CryptClient : IDisposable
    {
        internal CryptClient(MongoCryptSafeHandle handle)
        {
            this._handle = handle;
        }

        public void Foo()
        {
            Console.WriteLine("Hi");
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(_handle.IsClosed)
            {
                _handle.Dispose();
            }
        }
        #endregion
        MongoCryptSafeHandle _handle;
    }
}