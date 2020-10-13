using System;
using System.Linq;

namespace MongoDB.Libmongocrypt
{
    /// <summary>
    /// KMS Credentials
    /// </summary>
    /// <seealso cref="IKmsCredentials" />
    /// <seealso cref="IInternalKmsCredentials" />
    public class KmsCredentials : IKmsCredentials, IInternalKmsCredentials
    {
        #region static
        private static readonly KmsType[] __supportedKmsTypes = new[]
        {
            KmsType.Local,
            KmsType.Aws,
            KmsType.Azure,
            KmsType.Gcp
        };
        #endregion

        private byte[] _credentialsBytes;
        public KmsType _kmsType;

        /// <summary>
        /// Creates an <see cref="KmsCredentials"/> class.
        /// </summary>
        /// <param name="kmsType">The kms type.</param>
        /// <param name="credentialsBytes">The credentials document in a binary form.</param>
        public KmsCredentials(KmsType kmsType, byte[] credentialsBytes)
        {
            _credentialsBytes = credentialsBytes ?? throw new ArgumentNullException(nameof(credentialsBytes));
            _kmsType = kmsType;
            EnsureThatKmsTypeIsSupported(_kmsType);
        }

        public KmsType KmsType => _kmsType;

        // private methods
        private void EnsureThatKmsTypeIsSupported(KmsType kmsType)
        {
            if (!__supportedKmsTypes.Contains(kmsType))
            {
                throw new NotSupportedException($"The provided kms type {kmsType} is not supported.");
            }
        }

        void IInternalKmsCredentials.SetCredentials(MongoCryptSafeHandle handle, Status status)
        {
            unsafe
            {
                fixed (byte* p = _credentialsBytes)
                {
                    IntPtr ptr = (IntPtr)p;
                    using (PinnedBinary pinned = new PinnedBinary(ptr, (uint)_credentialsBytes.Length))
                    {
                        handle.Check(status, Library.mongocrypt_setopt_kms_providers(handle, pinned.Handle));
                    }
                }
            }
        }
    }
}
