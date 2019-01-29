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
    public class Library
    {
        static Library()
        {
            LibraryLoader loader = new LibraryLoader();

            mongocrypt_init = loader.getFunction<Delegates.mongocrypt_init>("mongocrypt_init");
            mongocrypt_version = loader.getFunction<Delegates.mongocrypt_version>("mongocrypt_version");
        }

        public static string GetVersion()
        {
            IntPtr p = mongocrypt_version();
            return Marshal.PtrToStringAnsi(p);
        }

        public static readonly Delegates.mongocrypt_init mongocrypt_init;
        public static readonly Delegates.mongocrypt_version mongocrypt_version;

        public class Delegates
        {
            public delegate void mongocrypt_init();
            public delegate IntPtr mongocrypt_version();
        }

        class LibraryLoader
        {

            SharedLibraryLoader _loader;
            static string path = "/Users/mark/src/libmongocrypt/debug/libmongocrypt.dylib";
            public LibraryLoader()
            {

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // TOD
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    _loader = new DarwinLibrary(path);
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
        }

    }
}