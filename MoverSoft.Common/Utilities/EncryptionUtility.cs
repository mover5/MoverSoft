namespace MoverSoft.Common.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Pkcs;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography.Xml;
    using System.Text;
    using MoverSoft.Common.Extensions;

    public static class EncryptionUtility
    {
        #region Constructor

        private static readonly AlgorithmIdentifier Aes128EncryptionAlgorithm = new AlgorithmIdentifier(new Oid("2.16.840.1.101.3.4.1.2"), keyLength: 128);

        private static readonly AlgorithmIdentifier Aes256EncryptionAlgorithm = new AlgorithmIdentifier(new Oid("2.16.840.1.101.3.4.1.42"), keyLength: 256);
        
        private static readonly AesCryptoServiceProvider Aes128EncryptionProvider = new AesCryptoServiceProvider { KeySize = 128, Mode = CipherMode.CBC };

        private static readonly AesCryptoServiceProvider Aes256EncryptionProvider = new AesCryptoServiceProvider { KeySize = 256, Mode = CipherMode.CBC };

        #endregion

        #region Encrypt

        public static string Encrypt(string plainText, string certificateThumbprint)
        {
            var encryptedData = EncryptionUtility.Encrypt(UTF8Encoding.UTF8.GetBytes(plainText), certificateThumbprint);
            return Convert.ToBase64String(encryptedData);
        }

        public static byte[] Encrypt(byte[] rawData, string certificateThumbprint)
        {
            var certificate = StoreLocation.LocalMachine.FindCertificateByThumbprint(certificateThumbprint);
            EnvelopedCms cms = new EnvelopedCms(contentInfo: new ContentInfo(rawData), encryptionAlgorithm: EncryptionUtility.Aes128EncryptionAlgorithm);
            CmsRecipient recipient = new CmsRecipient(certificate);
            cms.Encrypt(recipient);

            return cms.Encode();
        }

        #endregion

        #region Decrypt

        public static string DecryptSecrets(string encryptedSecret)
        {
            if (!string.IsNullOrWhiteSpace(encryptedSecret))
            {
                var encryptedBytes = Convert.FromBase64String(encryptedSecret);
                var decryptedBytes = EncryptionUtility.DecryptSecrets(encryptedBytes);

                if (decryptedBytes.Any())
                {
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }

            return string.Empty;
        }

        public static byte[] DecryptSecrets(byte[] encryptedBytes)
        {
            if (encryptedBytes != null && encryptedBytes.Any())
            {
                EnvelopedCms cms = new EnvelopedCms();
                cms.Decode(encryptedBytes);

                if (cms.ContentEncryptionAlgorithm.Oid.Value == EncryptionUtility.Aes128EncryptionAlgorithm.Oid.Value ||
                    cms.ContentEncryptionAlgorithm.Oid.Value == EncryptionUtility.Aes256EncryptionAlgorithm.Oid.Value)
                {
                    try
                    {
                        foreach (var recipientInfo in cms.RecipientInfos
                            .Cast<RecipientInfo>()
                            .Where(recipientInfo => recipientInfo.RecipientIdentifier.Type == SubjectIdentifierType.IssuerAndSerialNumber))
                        {
                            var serialNumber = recipientInfo.RecipientIdentifier.Value.Cast<X509IssuerSerial>().SerialNumber;
                            var certificate = StoreLocation.LocalMachine.FindCertificateBySerialNumber(serialNumber);

                            if (certificate != null && certificate.HasPrivateKey)
                            {
                                var key = ((RSACryptoServiceProvider)certificate.PrivateKey).Decrypt(recipientInfo.EncryptedKey, fOAEP: true);
                                var iv = cms.ContentEncryptionAlgorithm.Parameters.Skip(2).ToArray();

                                using (var buffer = new MemoryStream(cms.ContentInfo.Content.Length))
                                using (var decryptor = cms.ContentEncryptionAlgorithm.Oid.Value == EncryptionUtility.Aes128EncryptionAlgorithm.Oid.Value
                                    ? EncryptionUtility.Aes128EncryptionProvider.CreateDecryptor(key, iv)
                                    : EncryptionUtility.Aes256EncryptionProvider.CreateDecryptor(key, iv))
                                {
                                    using (var cryptoStream = new CryptoStream(buffer, decryptor, CryptoStreamMode.Write))
                                    {
                                        cryptoStream.Write(cms.ContentInfo.Content, 0, cms.ContentInfo.Content.Length - 6);
                                    }

                                    return buffer.ToArray();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.IsFatal())
                        {
                            throw;
                        }
                    }
                }

                try
                {
                    cms.Decrypt();
                }
                catch (CryptographicException ex)
                {
                    throw new InvalidOperationException(string.Format(
                        format: "The secret could not be decrypted. Check that certificate with serial number '{0}' (issuer '{1}') is installed. The inner cryptographic exception: '{2}'.",
                        arg0: EncryptionUtility.GetDecryptionCertificates(cms).Any() ? EncryptionUtility.GetDecryptionCertificates(cms).Single().SerialNumber : "<null>",
                        arg1: EncryptionUtility.GetDecryptionCertificates(cms).Any() ? EncryptionUtility.GetDecryptionCertificates(cms).Single().IssuerName : "<null>",
                        arg2: ex.Message));
                }

                return cms.ContentInfo.Content;
            }

            return new byte[0];
        }

        #endregion

        #region CanDecrypt

        private static bool CanDecrypt(string encryptedSecret, string serialNumber)
        {
            var certificates = EncryptionUtility.GetDecryptionCertificates(encryptedSecret);
            return certificates.Any(certificate => certificate.SerialNumber.Equals(serialNumber));
        }

        private static bool CanDecrypt(byte[] encryptedBytes, string serialNumber)
        {
            var certificates = EncryptionUtility.GetDecryptionCertificates(encryptedBytes);
            return certificates.Any(certificate => certificate.SerialNumber.Equals(serialNumber));
        }

        private static IList<X509IssuerSerial> GetDecryptionCertificates(string encryptedSecret)
        {
            if (!string.IsNullOrWhiteSpace(encryptedSecret))
            {
                var encryptedBytes = Convert.FromBase64String(encryptedSecret);
                return EncryptionUtility.GetDecryptionCertificates(encryptedBytes);
            }

            return new X509IssuerSerial[0];
        }

        private static IList<X509IssuerSerial> GetDecryptionCertificates(byte[] encryptedBytes)
        {
            if (encryptedBytes != null && encryptedBytes.Any())
            {
                EnvelopedCms cms = new EnvelopedCms();
                cms.Decode(encryptedBytes);

                return EncryptionUtility.GetDecryptionCertificates(cms);
            }

            return new X509IssuerSerial[0];
        }

        private static IList<X509IssuerSerial> GetDecryptionCertificates(EnvelopedCms cms)
        {
            if (cms.RecipientInfos != null)
            {
                return cms.RecipientInfos
                    .Cast<RecipientInfo>()
                    .Where(recipient => recipient.RecipientIdentifier.Type == SubjectIdentifierType.IssuerAndSerialNumber)
                    .Select(recipient => recipient.RecipientIdentifier.Value)
                    .OfType<X509IssuerSerial>()
                    .ToList();
            }

            return new X509IssuerSerial[0];
        }

        #endregion
    }
}
