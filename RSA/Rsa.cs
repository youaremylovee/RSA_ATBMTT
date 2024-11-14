using System.Numerics;
using System.Text;

namespace MyRSA
{
    public class Rsa
    {
        public Rsa() { }

        public BigInteger Value { get; set; } = 0;
        public BigInteger N { get; set; } = 0; 
        public RsaChain RsaChain { get; set; } = new();

        public static RsaChain Create(BigInteger p, BigInteger q, int certainty)
        {
            if (!Utils.IsPrimeMillerRabin(q, certainty) || !Utils.IsPrimeMillerRabin(p, certainty) || p == q)
            {
                throw new InvalidDataException("p and q must be prime and different");
            }

            RsaChain chain = new RsaChain();
            chain = chain.SetP(p).SetQ(q);
            return chain;
        }

        public static RsaChain Create()
        {
            RsaChain chain = new RsaChain();
            return chain;
        }

        private BigInteger DoPow(BigInteger input)
        {
            return BigInteger.ModPow(input, Value, N);
        }
        /// <summary>
        /// Mã hóa bảo mật - Sử dụng khóa công khai người nhận
        /// </summary>
        /// <param name="input">Chuỗi dữ liệu cần mã hóa</param>
        /// <returns>Dữ liệu dạng base64</returns>
        public string EncryptSecure(string input)
        {
            BigInteger inputData = new BigInteger(Encoding.UTF8.GetBytes(input));
            BigInteger encryptedData = DoPow(inputData); 
            return Convert.ToBase64String(encryptedData.ToByteArray());
        }
        /// <summary>
        /// Mã hóa bảo mật - Sử dụng khóa công khai người nhận
        /// </summary>
        /// <param name="input">Mảng byte đầu vào</param>
        /// <returns>Mảng byte đã mã hóa</returns>
        public byte[] EncryptSecure(byte[] input)
        {
            BigInteger inputData = new BigInteger(input);
            BigInteger encryptedData = DoPow(inputData);
            return encryptedData.ToByteArray();
        }
        /// <summary>
        /// Mã hóa chữ ký - Sử dụng khóa riêng người gửi
        /// </summary>
        /// <param name="input">Chuỗi dữ liệu cần mã hóa</param>
        /// <returns>Dữ liệu dạng base64</returns>
        public string EncryptSignature(string input)
        {
            BigInteger inputData = new BigInteger(Encoding.UTF8.GetBytes(input));
            BigInteger signature = DoPow(inputData); 
            return Convert.ToBase64String(signature.ToByteArray());
        }
        /// <summary>
        /// Giải mã bảo mật - Sử dụng khóa riêng người nhận
        /// </summary>
        /// <param name="input">Chuỗi dữ liệu cần giải mã</param>
        /// <returns>Kết quả giải mã</returns>
        public string DecryptSecure(string input)
        {
            BigInteger encryptedData = new BigInteger(Convert.FromBase64String(input));
            BigInteger decryptedData = DoPow(encryptedData); 
            return Encoding.UTF8.GetString(decryptedData.ToByteArray());
        }
        /// <summary>
        /// Giải mã bảo mật - Sử dụng khóa riêng người nhận
        /// </summary>
        /// <param name="input">Mảng byte đầu vào</param>
        /// <returns>Mảng byte đã giải mã</returns>
        public byte[] DecryptSecure(byte[] input)
        {
            BigInteger inputData = new BigInteger(input);
            BigInteger decryptedData = DoPow(inputData);
            return decryptedData.ToByteArray();
        }
        /// <summary>
        /// Giải mã chữ ký - Sử dụng khóa công khai người gửi
        /// </summary>
        /// <param name="input">Chuỗi dữ liệu cần giải mã</param>
        /// <returns>Kết quả giải mã</returns>
        public string DecryptSignature(string input)
        {
            BigInteger signature = new BigInteger(Convert.FromBase64String(input));
            BigInteger originalData = DoPow(signature); 
            return Encoding.UTF8.GetString(originalData.ToByteArray());
        }
    }
}
