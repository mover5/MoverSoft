namespace MoverSoft.Common.Extensions
{
    using System;
    using System.Collections.Concurrent;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Extension methods to find extensions in the certificate store
    /// </summary>
    public static class CertificateExtensions
    {
        /// <summary>
        /// This caches all certificates we load.
        /// </summary>
        private static readonly ConcurrentDictionary<string, X509Certificate2> CachedCertificates = new ConcurrentDictionary<string, X509Certificate2>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Finds the certificate in the certificate store by thumbprint.
        /// </summary>
        /// <param name="location">The location of the certificate store.</param>
        /// <param name="thumbprint">The thumbprint of the certificate.</param>
        /// <param name="name">The name of the certificate store.</param>
        public static X509Certificate2 FindCertificateByThumbprint(this StoreLocation location, string thumbprint, StoreName name = StoreName.My)
        {
            return CertificateExtensions.FindCertificate(
                storeLocation: location,
                storeName: name,
                findType: X509FindType.FindByThumbprint,
                findValue: thumbprint,
                validOnly: false);
        }

        /// <summary>
        /// Finds the certificate in the certificate store by serial number.
        /// </summary>
        /// <param name="location">The location of the certificate store.</param>
        /// <param name="serialNumber">The serial number of the certificate.</param>
        /// <param name="name">The name of the certificate store.</param>
        public static X509Certificate2 FindCertificateBySerialNumber(this StoreLocation location, string serialNumber, StoreName name = StoreName.My)
        {
            return CertificateExtensions.FindCertificate(
                storeLocation: location,
                storeName: name,
                findType: X509FindType.FindBySerialNumber,
                findValue: serialNumber,
                validOnly: false);
        }

        /// <summary>
        /// Finds the certificate.
        /// </summary>
        /// <param name="storeLocation">The store location.</param>
        /// <param name="storeName">Name of the store.</param>
        /// <param name="findType">Type of the find.</param>
        /// <param name="findValue">The find value.</param>
        /// <param name="validOnly">Return only valid certificates if set to true.</param>
        private static X509Certificate2 FindCertificate(StoreLocation storeLocation, StoreName storeName, X509FindType findType, string findValue, bool validOnly)
        {
            return CertificateExtensions.CachedCertificates.GetOrAdd(
                key: findType + '#' + findValue,
                valueFactory: ignored =>
                {
                    using (var store = new X509Store2(storeName, storeLocation, flags: OpenFlags.ReadOnly))
                    {
                        var certificates = store.Certificates.Find(findType: findType, findValue: findValue, validOnly: validOnly);
                        return certificates.Count >= 1 ? certificates[0] : null;
                    }
                });
        }

        /// <summary>
        /// IDisposable X509Store wrapper.
        /// </summary>
        private class X509Store2 : IDisposable
        {
            /// <summary>
            /// The X509 store.
            /// </summary>
            private readonly X509Store store;

            /// <summary>
            /// Initializes a new instance of the <see cref="X509Store2"/> class.
            /// </summary>
            /// <param name="storeName">Name of the store.</param>
            /// <param name="storeLocation">The store location.</param>
            /// <param name="flags">The store open flags.</param>
            public X509Store2(StoreName storeName, StoreLocation storeLocation, OpenFlags flags)
            {
                this.store = new X509Store(storeName, storeLocation);
                this.store.Open(flags);
            }

            /// <summary>
            /// Gets the certificates.
            /// </summary>
            public X509Certificate2Collection Certificates
            {
                get { return this.store.Certificates; }
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                this.store.Close();
            }
        }
    }
}
