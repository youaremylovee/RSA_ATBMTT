using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using SautinSoft.Pdf;
using SautinSoft.Pdf.Forms;
using Org.BouncyCastle.X509;
using System.Collections;
using System.Text;

namespace MyRSA
{
    public class PdfExtractor
    {
        public static Org.BouncyCastle.Math.BigInteger? GetSerialNumber(Stream stream)
        {
            using (PdfDocument document = PdfDocument.Load(stream))
            {
                var field = document.Form.Fields.FirstOrDefault(f => f is PdfSignatureField);
                if (field != null && field is PdfSignatureField signField)
                {
                    var content = signField.Value.Content;
                    Org.BouncyCastle.X509.X509Certificate certificate = new Org.BouncyCastle.X509.X509Certificate(content.SignerCertificate.GetRawData());
                    return certificate.SerialNumber;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
