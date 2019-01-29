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
    public class Stuff
    {
        public static void Init()
        {
            Console.WriteLine("Initializing...");

             Console.WriteLine("Version: " + LibMongoCrypt.GetVersion());
        }
    }
    /*
     * Windows:
     * https://stackoverflow.com/questions/2864673/specify-the-search-path-for-dllimport-in-net
     *
     * See for better ways
     * https://github.com/dotnet/coreclr/issues/930
     * https://github.com/dotnet/corefx/issues/32015
     *
     */
    internal class LibMongoCrypt
    {
        static  LibMongoCrypt() {
            LibraryLoader.mongocrypt_init();
        }

        public static string GetVersion() {
            IntPtr p = LibraryLoader.mongocrypt_version();
             return Marshal.PtrToStringAnsi(p);
        }

        class LibraryLoader
        {

            static string path = "/Users/mark/src/libmongocrypt/debug/libmongocrypt.dylib";
            static LibraryLoader()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // TOD
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    IntPtr handle = DarwinLibray.dlopen(path, DarwinLibray.RTLD_GLOBAL | DarwinLibray.RTLD_NOW);
                    Console.WriteLine("handle : " + handle);
                    if(handle == IntPtr.Zero) {
                        throw new NotImplementedException();
                    }

                    IntPtr a1 = DarwinLibray.dlsym(handle, "mongocrypt_init");
                    Console.WriteLine("handle : " + a1);
                    mongocrypt_init = Marshal.GetDelegateForFunctionPointer<Delegates.mongocrypt_init>(a1);
                    IntPtr a2 = DarwinLibray.dlsym(handle, "mongocrypt_version");
                    Console.WriteLine("handle : " + a2);
                    mongocrypt_version = Marshal.GetDelegateForFunctionPointer<Delegates.mongocrypt_version>(a2);
                }
            }

            public static readonly Delegates.mongocrypt_init mongocrypt_init;
            public static readonly Delegates.mongocrypt_version mongocrypt_version;

            public class Delegates {
            public delegate void mongocrypt_init();
            public delegate IntPtr mongocrypt_version();
            }
            class DarwinLibray
            {

                // See dlfcn.h
                // #define RTLD_LAZY       0x1
                // #define RTLD_NOW        0x2
                // #define RTLD_LOCAL      0x4
                // #define RTLD_GLOBAL     0x8
                public const int RTLD_GLOBAL = 0x8;
                public const int RTLD_NOW = 0x2;

                [DllImport("libdl")]
                public static extern IntPtr dlopen(string filename, int flags);

                [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, CharSet =CharSet.Ansi)]
                public static extern IntPtr dlsym(IntPtr handle, string symbol);
            }
        }

    }
}