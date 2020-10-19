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

#if !NET452
using System;
using System.Runtime.InteropServices;
#endif

namespace MongoDB.Libmongocrypt
{
    internal enum OperationSystemPlatform
    {
        Windows,
        Linux,
        MacOS
    }

    internal static class OperationSystemHelper
    {
        public static OperationSystemPlatform CurrentOperationSystem
        {
            get
            {
#if !NET452
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return OperationSystemPlatform.MacOS;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return OperationSystemPlatform.Linux;
                }
                else if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // should not be reached. If we're here, then there is a bug in the library
                    throw new PlatformNotSupportedException($"Unexpected os platform {RuntimeInformation.OSDescription}.");
                }
                else
#endif
                return OperationSystemPlatform.Windows;
            }
        }
    }
}
