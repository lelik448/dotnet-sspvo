using GostCryptography.Pkcs;
using System;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp2
{
    public class Crypto
    {
        private static X509Certificate2 _certificate;

        public X509Certificate2 GetCertificate()
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                var collection = (X509Certificate2Collection)store.Certificates;
                var fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                foreach (X509Certificate2 x509 in fcollection)
                {
                    if (x509.Subject.IndexOf(X509SubjectFragment, StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return x509;
                    }
                }
            }

            return null;
        }

        public string X509SubjectFragment { get; set; }

        public byte[] SignData(byte[] dataToSign)
        {
            _certificate = _certificate ?? GetCertificate();
            return SignData(_certificate, dataToSign, true);
        }

        public byte[] SignData(X509Certificate2 certificate, byte[] dataToSign, bool detached)
        {
            var contentInfo = new ContentInfo(dataToSign);
            // Создание объекта для подписи сообщения
            var cms = new GostSignedCms(contentInfo, detached);
            // Создание объект с информацией о подписчике
            var signer = new CmsSigner(certificate);
            // Создание подписи для сообщения CMS/PKCS#7
            cms.ComputeSignature(signer, false);
            // Создание подписи CMS/PKCS#7
            return cms.Encode();
        }
    }
}
