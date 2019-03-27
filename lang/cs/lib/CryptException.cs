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
    public class CryptException : Exception
    {
        internal CryptException(Library.ErrorType errorType, UInt32 code, string message) : base(message)
        {
            _code = code;
            _errorType = errorType;
        }

        // TODO - expose codes

        private readonly UInt32 _code;
        private readonly Library.ErrorType _errorType;
    }
}