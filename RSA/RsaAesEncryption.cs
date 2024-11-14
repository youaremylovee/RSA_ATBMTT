using System;
using System.IO;
using System.Security.Cryptography;
using System.Numerics;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Utilities.Zlib;
namespace MyRSA
{
    public class RsaAesEncryption
    {
        public static void EncryptFileWithAesAndRsa(byte[] fileData, string outputEnc, string outputKey, Rsa rsa)
        {
            // Step 1: Generate AES-192 key and IV
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 192; // AES-192
                aes.GenerateKey();
                aes.GenerateIV();

                // Step 2: Encrypt the data using AES
                byte[] encryptedData;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(fileData, 0, fileData.Length);
                        cs.Close();
                    }
                    encryptedData = ms.ToArray();
                }

                // Step 3: Encrypt the AES key using RSA public key
                byte[] encryptedAesKey = rsa.EncryptSecure(aes.Key);

                // Step 4: Save the encrypted AES key, IV, and the encrypted data
                using (MemoryStream outputStream = new MemoryStream())
                {
                    // Lưu AES IV và encrypted AES key vào tệp khóa
                    outputStream.Write(aes.IV, 0, aes.IV.Length);
                    outputStream.Write(encryptedAesKey, 0, encryptedAesKey.Length);
                    // Lưu vào tệp outputKey
                    File.WriteAllBytes(outputKey, outputStream.ToArray());
                }

                using (MemoryStream outputStream = new MemoryStream())
                {
                    // Lưu dữ liệu đã mã hóa vào tệp dữ liệu
                    outputStream.Write(encryptedData, 0, encryptedData.Length);
                    // Lưu vào tệp outputEnc
                    File.WriteAllBytes(outputEnc, outputStream.ToArray());
                }
            }
        }

        public static void DecryptFileWithAesAndRsa(byte[] encKey, byte[] encryptedData, string outputFile, Rsa rsa)
        {
            // Step 1: Read the encrypted key, IV, and encrypted data from the input files
            byte[] aesIV = new byte[16];
            byte[] keyAes = new byte[encKey.Length - 16];
            Array.Copy(encKey, 0, aesIV, 0, 16);
            Array.Copy(encKey, 16, keyAes, 0, keyAes.Length);
            // Step 2: Decrypt the AES key using the RSA private key
            byte[] aesKey = rsa.DecryptSecure(keyAes);

            // Step 3: Decrypt the data using AES
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 192; // AES-192
                aes.Key = aesKey;
                aes.IV = aesIV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedData, 0, encryptedData.Length);
                        cs.Close();
                    }
                    byte[] decryptedData = ms.ToArray();

                    // Step 4: Save the decrypted data to the output file
                    File.WriteAllBytes(outputFile, decryptedData);
                }
            }
        }

    }
}
