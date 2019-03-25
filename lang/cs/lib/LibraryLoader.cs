using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MongoDB.MongoCrypt
{
    /*
     * Windows:
     * https://stackoverflow.com/questions/2864673/specify-the-search-path-for-dllimport-in-net
     *
     * See for better ways
     * https://github.com/dotnet/coreclr/issues/930
     * https://github.com/dotnet/corefx/issues/32015
     *
     */
    internal class LibraryLoader
    {
        SharedLibraryLoader _loader;

        public LibraryLoader()
        {
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

        public T getFunction<T>(string name)
        {
            IntPtr ptr = _loader.getFunction(name);
            if(ptr != IntPtr.Zero)
            {
                throw new FunctionNotFoundException(name);
            }

            return Marshal.GetDelegateForFunctionPointer<T>(ptr);

        }

        interface SharedLibraryLoader
        {
            IntPtr getFunction(string name);
        }

        public class FunctionNotFoundException : Exception
        {
            public FunctionNotFoundException(string message) : base(message) { }
        }

        class DarwinLibrary : SharedLibraryLoader
        {

            // See dlfcn.h
            // #define RTLD_LAZY       0x1
            // #define RTLD_NOW        0x2
            // #define RTLD_LOCAL      0x4
            // #define RTLD_GLOBAL     0x8
            public const int RTLD_GLOBAL = 0x8;
            public const int RTLD_NOW = 0x2;

            IntPtr _handle;
            public DarwinLibrary(string path)
            {

                _handle = dlopen(path, RTLD_GLOBAL | RTLD_NOW);
                if (_handle == IntPtr.Zero)
                {
                    throw new FileNotFoundException(path);
                }

            }

            public IntPtr getFunction(string name)
            {
                return dlsym(_handle, name);
            }

            [DllImport("libdl")]
            public static extern IntPtr dlopen(string filename, int flags);

            [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
        }


        class LinuxLibrary : SharedLibraryLoader
        {

            // See dlfcn.h
            // #define RTLD_LAZY       0x1
            // #define RTLD_NOW        0x2
            // #define RTLD_LOCAL      0x4
            // #define RTLD_GLOBAL     0x100
            public const int RTLD_GLOBAL = 0x100;
            public const int RTLD_NOW = 0x2;

            IntPtr _handle;
            public LinuxLibrary(string path)
            {

                _handle = dlopen(path, RTLD_GLOBAL | RTLD_NOW);
                if (_handle == IntPtr.Zero)
                {
                    throw new FileNotFoundException(path);
                }

            }

            public IntPtr getFunction(string name)
            {
                return dlsym(_handle, name);
            }

            [DllImport("libdl")]
            public static extern IntPtr dlopen(string filename, int flags);

            [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
        }


        class WindowsLibrary : SharedLibraryLoader
        {
            IntPtr _handle;
            public WindowsLibrary(string path)
            {

                _handle = LoadLibrary(path);
                if (_handle == IntPtr.Zero)
                {
                    //Marshal.GetLastWin32Error();
                    throw new FileNotFoundException(path);
                }

            }

            public IntPtr getFunction(string name)
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