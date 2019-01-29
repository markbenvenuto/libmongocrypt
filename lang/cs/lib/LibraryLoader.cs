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
        //            static string path = "/Users/mark/src/libmongocrypt/debug/libmongocrypt.dylib";
        //            static string path = "/home/mark/src/libmongocrypt/debug/libmongocrypt.so";
        public LibraryLoader()
        {

            // PS - I hate .net standard 1.5,
            // TODO We should use GetExecutingAssembly here
            //var location = Assembly.GetExecutingAssembly().Location;
            var assembly = typeof(LibraryLoader).GetTypeInfo().Assembly;
            var location = assembly.Location;
            string path = Path.GetDirectoryName(location);
            Console.WriteLine("Base Path: " + path);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // TOD
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _loader = new DarwinLibrary(path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Assembly executes from here: 
                // /home/mark/.nuget/packages/mongodb.crypt/1.0.0/lib/netstandard2.0
                path += "/../../native/linux/libmongocrypt.so";
                Console.WriteLine("Load path: " + path);
                _loader = new LinuxLibrary(path);
            }

        }

        public T getFunction<T>(string name)
        {
            IntPtr a2 = _loader.getFunction(name);
            Console.WriteLine("_handle : " + a2);
            return Marshal.GetDelegateForFunctionPointer<T>(a2);

        }


        interface SharedLibraryLoader
        {
            IntPtr getFunction(string name);
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
                Console.WriteLine("handle : " + _handle);
                if (_handle == IntPtr.Zero)
                {
                    throw new NotImplementedException();
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
                Console.WriteLine("handle : " + _handle);
                if (_handle == IntPtr.Zero)
                {
                    throw new NotImplementedException();
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
    }
}