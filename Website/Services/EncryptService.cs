using MyRSA;
using Org.BouncyCastle.Asn1.X9;
using RSA_UI.Models.Entity;
using SautinSoft.Pdf.Content;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Website.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Website.Services
{
    public class EncryptService
    {
        public string EncryptSecure(IFormFile data,string publicKey)
        {
            Rsa rsaPublicKey = Rsa.Create().FromPemContent(publicKey).Build();
            string taskId = Guid.NewGuid().ToString();
            using (var memoryStream = new MemoryStream())
            {
                data.CopyTo(memoryStream);
                var fileOutput = Path.Combine(Directory.GetCurrentDirectory(), "Tasks", taskId + data.FileName);
                var keyOutput = Path.Combine(Directory.GetCurrentDirectory(), "Tasks", taskId + ".aes" );
                byte[] fileData = memoryStream.ToArray();
                RsaAesEncryption.EncryptFileWithAesAndRsa(fileData, fileOutput, keyOutput, rsaPublicKey);
                return taskId;
            }
        }
        public async Task<string> EncryptSignature(IFormFile data, IFormFile privateKey, string authorID)
        {
            using(var stream = new MemoryStream()) 
            {
                await privateKey.CopyToAsync(stream);
                string hashSha256 = await Encryption.GetFileHash(data) ?? "";
                byte[] pemBytes = stream.ToArray();
                string pemContent = Encoding.UTF8.GetString(pemBytes);
                Rsa rsaPrivate = Rsa.Create().FromPemContent(pemContent).Build();
                string hashSha256WithRSA = rsaPrivate.EncryptSignature(hashSha256);
                string taskId = Guid.NewGuid().ToString();
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Tasks", taskId + data.FileName);
                var keyPath = Path.Combine(Directory.GetCurrentDirectory(), "Tasks", taskId + ".hash");
                string keyContent = $"Author ID: {authorID}\nHashSHA256: {hashSha256}\nSignature: {hashSha256WithRSA}";
                File.WriteAllText(keyPath, keyContent);
                //save file 
                using (var memoryStream = new MemoryStream())
                {
                    data.CopyTo(memoryStream);
                    byte[] fileData = memoryStream.ToArray();
                    File.WriteAllBytes(filePath, fileData);
                }
                return taskId;
            }
        }
        public async Task<string> DecryptSecure(IFormFile data, IFormFile privateKey, IFormFile KeyAes)
        {
            using(var stream = new MemoryStream())
            {
                await privateKey.CopyToAsync(stream);
                byte[] pemBytes = stream.ToArray();
                string pemContent = Encoding.UTF8.GetString(pemBytes);
                Rsa rsaPrivateKey = Rsa.Create().FromPemContent(pemContent).Build();

                byte[] encKey;
                byte[] encryptedData;

                using (var keyStream = KeyAes.OpenReadStream())
                {
                    encKey = new byte[KeyAes.Length];
                    keyStream.Read(encKey, 0, encKey.Length);
                }

                using (var dataStream = data.OpenReadStream())
                {
                    encryptedData = new byte[data.Length];
                    dataStream.Read(encryptedData, 0, encryptedData.Length);
                }

                string taskId = Guid.NewGuid().ToString();
                var pathOutput = Path.Combine(Directory.GetCurrentDirectory(), "Tasks", taskId + data.FileName);
                RsaAesEncryption.DecryptFileWithAesAndRsa(encKey, encryptedData, pathOutput, rsaPrivateKey);
                return taskId;
            }
        }
        public async Task<bool> DecryptSignature(IFormFile DocumentFile, string publicKey, string SignatureCompany)
        {
            var hash256 = await Encryption.GetFileHash(DocumentFile);
            Rsa rsaPublicKey = Rsa.Create().FromPemContent(publicKey).Build();
            string hash256Decrypt = rsaPublicKey.DecryptSignature(SignatureCompany);
            return hash256 == hash256Decrypt;
        }
    }
}
