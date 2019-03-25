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
using Microsoft.Win32.SafeHandles;
//using System.Runtime.ConstrainedExecution;


//using System.Security.Permissions;

namespace MongoDB.MongoCrypt
{
    //[SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    //[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    internal class MongoCryptSafeHandle : SafeHandle
    {
        private MongoCryptSafeHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get
            {
                return this.handle == IntPtr.Zero;
            }
        }

        // TODO: This is .NET Standard >= 2.0
        //[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            // Here, we must obey all rules for constrained execution regions.
            Library.mongocrypt_destroy(this.handle);
            return true;
            // If ReleaseHandle failed, it can be reported via the
            // "releaseHandleFailed" managed debugging assistant (MDA).  This
            // MDA is disabled by default, but can be enabled in a debugger
            // or during testing to diagnose handle corruption problems.
            // We do not throw an exception because most code could not recover
            // from the problem.
        }
    }

    internal class OptionsSafeHandle : SafeHandle
    {
        private OptionsSafeHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get
            {
                return this.handle == IntPtr.Zero;
            }
        }

        // TODO: This is .NET Standard >= 2.0
        //[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            // Here, we must obey all rules for constrained execution regions.
            Library.mongocrypt_opts_destroy(this.handle);
            return true;
            // If ReleaseHandle failed, it can be reported via the
            // "releaseHandleFailed" managed debugging assistant (MDA).  This
            // MDA is disabled by default, but can be enabled in a debugger
            // or during testing to diagnose handle corruption problems.
            // We do not throw an exception because most code could not recover
            // from the problem.
        }
    }

    internal class StatusSafeHandle : SafeHandle
    {
        private StatusSafeHandle()
            : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get
            {
                return this.handle == IntPtr.Zero;
            }
        }

        // TODO: This is .NET Standard >= 2.0
        //[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            // Here, we must obey all rules for constrained execution regions.
            Library.mongocrypt_status_destroy(this.handle);
            return true;
            // If ReleaseHandle failed, it can be reported via the
            // "releaseHandleFailed" managed debugging assistant (MDA).  This
            // MDA is disabled by default, but can be enabled in a debugger
            // or during testing to diagnose handle corruption problems.
            // We do not throw an exception because most code could not recover
            // from the problem.
        }
    }


    public class CryptContext
    {

    }

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

    public class CryptException : Exception
    {
        public CryptException(Int32 code, string message) : base(message)
        {
            _code = code;
        }

        private Int32 _code;
    }

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

                throw new CryptException(statusCode, message);
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

    public class CryptClientFactory
    {
        public static CryptClient Create(CryptOptions options)
        {
            MongoCryptSafeHandle handle = Library.mongocrypt_new(options.Handle);

            return new CryptClient(handle);
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
    public class Library
    {
        static Library()
        {
            LibraryLoader loader = new LibraryLoader();

            mongocrypt_version = loader.getFunction<Delegates.mongocrypt_version>("mongocrypt_version");

            mongocrypt_new = loader.getFunction<Delegates.mongocrypt_new>("mongocrypt_new");
            mongocrypt_destroy = loader.getFunction<Delegates.mongocrypt_destroy>("mongocrypt_destroy");

            mongocrypt_opts_new = loader.getFunction<Delegates.mongocrypt_opts_new>("mongocrypt_opts_new");
            mongocrypt_opts_destroy = loader.getFunction<Delegates.mongocrypt_opts_destroy>("mongocrypt_opts_destroy");
            mongocrypt_opts_set_opt = loader.getFunction<Delegates.mongocrypt_opts_set_opt>("mongocrypt_opts_set_opt");

            mongocrypt_status_new = loader.getFunction<Delegates.mongocrypt_status_new>("mongocrypt_status_new");
            mongocrypt_status_destroy = loader.getFunction<Delegates.mongocrypt_status_destroy>("mongocrypt_status_destroy");
            mongocrypt_status_error_type = loader.getFunction<Delegates.mongocrypt_status_error_type>("mongocrypt_status_error_type");
            mongocrypt_status_code = loader.getFunction<Delegates.mongocrypt_status_code>("mongocrypt_status_code");
            mongocrypt_status_message = loader.getFunction<Delegates.mongocrypt_status_message>("mongocrypt_status_message");
            mongocrypt_status_ok = loader.getFunction<Delegates.mongocrypt_status_ok>("mongocrypt_status_ok");

        }

        public static string Version
        {
            get {
                IntPtr p = mongocrypt_version();
                return Marshal.PtrToStringAnsi(p);
            }
        }

        internal static readonly Delegates.mongocrypt_version mongocrypt_version;

        internal static readonly Delegates.mongocrypt_new mongocrypt_new;
        internal static readonly Delegates.mongocrypt_destroy mongocrypt_destroy;

        internal static readonly Delegates.mongocrypt_opts_new mongocrypt_opts_new;
        internal static readonly Delegates.mongocrypt_opts_destroy mongocrypt_opts_destroy;
        internal static readonly Delegates.mongocrypt_opts_set_opt mongocrypt_opts_set_opt;

        internal static readonly Delegates.mongocrypt_status_new mongocrypt_status_new;
        internal static readonly Delegates.mongocrypt_status_destroy mongocrypt_status_destroy;

        internal static readonly Delegates.mongocrypt_status_error_type mongocrypt_status_error_type;
        internal static readonly Delegates.mongocrypt_status_code mongocrypt_status_code;
        internal static readonly Delegates.mongocrypt_status_message mongocrypt_status_message;
        internal static readonly Delegates.mongocrypt_status_ok mongocrypt_status_ok;

        internal enum Options
        {
            MONGOCRYPT_AWS_REGION,
            MONGOCRYPT_AWS_SECRET_ACCESS_KEY,
            MONGOCRYPT_AWS_ACCESS_KEY_ID,
            MONGOCRYPT_LOG_FN,
            MONGOCRYPT_LOG_CTX
        }

        internal enum ErrorType {
            MONGOCRYPT_ERROR_TYPE_NONE = 0,
            MONGOCRYPT_ERROR_TYPE_MONGOCRYPTD,
            MONGOCRYPT_ERROR_TYPE_KMS,
            MONGOCRYPT_ERROR_TYPE_CLIENT
        }

        internal class Delegates
        {
            public delegate IntPtr mongocrypt_version();

            public delegate MongoCryptSafeHandle mongocrypt_new(OptionsSafeHandle ptr);
            public delegate void mongocrypt_destroy(IntPtr ptr);

            public delegate OptionsSafeHandle mongocrypt_opts_new();
            public delegate void mongocrypt_opts_destroy(IntPtr ptr);
            public delegate void mongocrypt_opts_set_opt(OptionsSafeHandle ptr, Options opts, IntPtr value);

            public delegate StatusSafeHandle mongocrypt_status_new();
            public delegate void mongocrypt_status_destroy(IntPtr ptr);
            public delegate ErrorType mongocrypt_status_error_type(StatusSafeHandle ptr);
            public delegate Int32 mongocrypt_status_code(StatusSafeHandle ptr);
            public delegate IntPtr mongocrypt_status_message(StatusSafeHandle ptr);
            public delegate bool mongocrypt_status_ok(StatusSafeHandle ptr);

        }
    }
}