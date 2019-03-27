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
using System.Reflection;
using System.Runtime.InteropServices;

namespace MongoDB.Crypt
{
    /// <summary>
    /// LibraryLoader abstracts loading C functions from a shared library across OS
    /// </summary>
    internal class LibraryLoader
    {
        ISharedLibraryLoader _loader;

        public LibraryLoader()
        {
            // Windows:
            // https://stackoverflow.com/questions/2864673/specify-the-search-path-for-dllimport-in-net
            //
            // See for better ways
            // https://github.com/dotnet/coreclr/issues/930
            // https://github.com/dotnet/corefx/issues/32015
            List<string> candidatePaths = new List<string>();

            // In the nuget package, get the shared library from a relative path of this assembly
            // Also, when running locally, get the shared library from a relative path of this assembly
            var assembly = typeof(LibraryLoader).GetTypeInfo().Assembly;
            var location = assembly.Location;
            string basepath = Path.GetDirectoryName(location);
            candidatePaths.Add(basepath);
            // TODO - .NET Standard 2.0
//            Trace.WriteLine("Base Path: " + basepath)

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string[] suffixPaths = new[]{
                    @"..\..\native\windows\",
                    ""};
                string path = FindLibrary(candidatePaths, suffixPaths, "mongocrypt.dll");
                _loader = new WindowsLibrary(path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string[] suffixPaths = new[]{
                    "../../native/osx/",
                    ""};
                string path = FindLibrary(candidatePaths, suffixPaths, "libmongocrypt.dylib");
                _loader = new DarwinLibrary(path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string[] suffixPaths = new[]{
                    "../../native/linux/",
                    ""};
                string path = FindLibrary(candidatePaths, suffixPaths, "libmongocrypt.so");
                _loader = new LinuxLibrary(path);
            }
        }

        private string FindLibrary(IList<string> basePaths, string[] suffixPaths, string library)
        {
            foreach (var basePath in basePaths)
            {
                foreach (var suffix in suffixPaths)
                {
                    string path = Path.Combine(basePath, suffix, library);
                    if (File.Exists(path))
                    {
                        // TODO - .NET Standard 2.0
                        //Trace.WriteLine("Load path: " + path);
                        return path;
                    }
                }
            }

            throw new FileNotFoundException("Could not find: " + library);
        }

        public T GetFunction<T>(string name)
        {
            IntPtr ptr = _loader.GetFunction(name);
            if(ptr == IntPtr.Zero)
            {
                throw new FunctionNotFoundException(name);
            }

            return Marshal.GetDelegateForFunctionPointer<T>(ptr);

        }

        public class FunctionNotFoundException : Exception
        {
            public FunctionNotFoundException(string message) : base(message) { }
        }

        private interface ISharedLibraryLoader
        {
            IntPtr GetFunction(string name);
        }

        /// <summary>
        /// macOS Dynamic Library loader using dlsym
        /// </summary>
        private class DarwinLibrary : ISharedLibraryLoader
        {

            // See dlfcn.h
            // #define RTLD_LAZY       0x1
            // #define RTLD_NOW        0x2
            // #define RTLD_LOCAL      0x4
            // #define RTLD_GLOBAL     0x8
            public const int RTLD_GLOBAL = 0x8;
            public const int RTLD_NOW = 0x2;

            readonly IntPtr _handle;
            public DarwinLibrary(string path)
            {

                _handle = dlopen(path, RTLD_GLOBAL | RTLD_NOW);
                if (_handle == IntPtr.Zero)
                {
                    throw new FileNotFoundException(path);
                }

            }

            public IntPtr GetFunction(string name)
            {
                return dlsym(_handle, name);
            }

#pragma warning disable IDE1006 // Naming Styles
            [DllImport("libdl")]
            public static extern IntPtr dlopen(string filename, int flags);

            [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
#pragma warning restore IDE1006 // Naming Styles
        }

        /// <summary>
        /// Linux Shared Object loader using dlsym
        /// </summary>
        class LinuxLibrary : ISharedLibraryLoader
        {

            // See dlfcn.h
            // #define RTLD_LAZY       0x1
            // #define RTLD_NOW        0x2
            // #define RTLD_LOCAL      0x4
            // #define RTLD_GLOBAL     0x100
            public const int RTLD_GLOBAL = 0x100;
            public const int RTLD_NOW = 0x2;

            readonly IntPtr _handle;
            public LinuxLibrary(string path)
            {

                _handle = dlopen(path, RTLD_GLOBAL | RTLD_NOW);
                if (_handle == IntPtr.Zero)
                {
                    throw new FileNotFoundException(path);
                }

            }

            public IntPtr GetFunction(string name)
            {
                return dlsym(_handle, name);
            }

#pragma warning disable IDE1006 // Naming Styles
            [DllImport("libdl")]
            public static extern IntPtr dlopen(string filename, int flags);

            [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
#pragma warning restore IDE1006 // Naming Styles
        }


        /// <summary>
        /// Windows DLL loader using GetProcAddress
        /// </summary>
        private  class WindowsLibrary : ISharedLibraryLoader
        {
            readonly IntPtr _handle;
            public WindowsLibrary(string path)
            {

                _handle = LoadLibrary(path);
                if (_handle == IntPtr.Zero)
                {
                    //TODO: Marshal.GetLastWin32Error();
                    throw new FileNotFoundException(path);
                }

            }

            public IntPtr GetFunction(string name)
            {
                return GetProcAddress(_handle, name);
            }

            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
            static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        }
    }
}