using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace MyRSA
{
	public class RsaChain
	{
		private bool _isFromFile = false;
		private BigInteger _p = 0;
		private BigInteger _q = 0;
        public BigInteger _privateKey = 0;
        public BigInteger _publicKey = 0;
        private BigInteger _value = 0;
        public BigInteger _n { get; private set; } = 0;

        public RsaChain SetP(BigInteger p)
		{
			this._p = p;
			return this;
		}

		public RsaChain SetQ(BigInteger q)
		{
			this._q = q;
			return this;
		}

		private BigInteger CalcD(int e, BigInteger phi)
		{
			BigInteger d, x, y;
			BigInteger g = ExtendedGCD(e, phi, out x, out y);

			if (g != 1)
			{
				throw new Exception("e và phi không nguyên tố cùng nhau, không thể tính d.");
			}

			d = (x % phi + phi) % phi;
			return d;
		}

		private BigInteger ExtendedGCD(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
		{
			if (a == 0)
			{
				x = 0;
				y = 1;
				return b;
			}

			BigInteger x1, y1;
			BigInteger gcd = ExtendedGCD(b % a, a, out x1, out y1);

			x = y1 - (b / a) * x1;
			y = x1;

			return gcd;
		}

		public Rsa Build()
		{
            Rsa _rsa = new Rsa();
            if (!_isFromFile)
			{
				BigInteger n = _p * _q;
				BigInteger phi = (_p - 1) * (_q - 1);
				int e = 65537;
				BigInteger d = CalcD(e, phi);
				this._privateKey = d;
				this._publicKey = e;
                this._n = n;
				_rsa.N = n;
			}
            else
            {
                _rsa.Value = _value;
                _rsa.N = _n;
            }
            _rsa.RsaChain = this;
			return _rsa;
		}

		// Phương thức đọc khóa từ tệp PEM
		public RsaChain FromPemFile(string pemFilePath)
		{
			string pemContent = File.ReadAllText(pemFilePath);
			string base64Key = Regex.Replace(pemContent, @"-----\w+ RSA (PRIVATE|PUBLIC) KEY-----|\s+", "");
			byte[] keyBytes = Convert.FromBase64String(base64Key);
			string keyStr = Encoding.UTF8.GetString(keyBytes);
			string[] keys = keyStr.Split(',');
			if (keys.Length != 2)
			{
				throw new FormatException("Invalid key format. Expected format is 'n,Value'.");
			}
			BigInteger n = BigInteger.Parse(keys[0]);
			BigInteger key = BigInteger.Parse(keys[1]);
			this._n = n;
            this._value = key;
			_isFromFile = true;
			return this;
		}
        public string GetPublicKey()
        {
            this._n = this._p * this._q;
            string keyStr = $"{this._n},{this._publicKey}";
            byte[] keyBytes = Encoding.UTF8.GetBytes(keyStr);
            string base64Key = Convert.ToBase64String(keyBytes, Base64FormattingOptions.InsertLineBreaks);
            StringBuilder sbContent = new StringBuilder();
            sbContent.AppendLine(base64Key);
            return sbContent.ToString();
        }
        public RsaChain FromPemContent(string pemContent)
        {
            string base64Key = Regex.Replace(pemContent, @"-----\w+ RSA (PRIVATE|PUBLIC) KEY-----|\s+", "");
            byte[] keyBytes = Convert.FromBase64String(base64Key);
            string keyStr = Encoding.UTF8.GetString(keyBytes);
            string[] keys = keyStr.Split(',');
            if (keys.Length != 2)
            {
                throw new FormatException("Invalid key format. Expected format is 'n,e'.");
            }
            BigInteger n = BigInteger.Parse(keys[0]);
            BigInteger key = BigInteger.Parse(keys[1]);
            this._n = n;
            this._value = key;
            _isFromFile = true;
            return this;
        }

        // Phương thức lưu khóa ra tệp PEM
        public void ToPemFile(string pemFilePath, bool isPrivateKey = true)
        {
            BigInteger key = isPrivateKey ? this._privateKey : this._publicKey;
            string keyStr = $"{this._n},{key}";
            byte[] keyBytes = Encoding.UTF8.GetBytes(keyStr);

            // Chuyển dữ liệu thành Base64 và chia thành dòng 64 ký tự
            string base64Key = Convert.ToBase64String(keyBytes, Base64FormattingOptions.InsertLineBreaks);

            // Tạo tiêu đề và footer PEM dựa trên loại khóa
            string pemHeader = isPrivateKey
                ? "-----BEGIN RSA PRIVATE KEY-----"
                : "-----BEGIN RSA PUBLIC KEY-----";
            string pemFooter = isPrivateKey
                ? "-----END RSA PRIVATE KEY-----"
                : "-----END RSA PUBLIC KEY-----";

            // Xây dựng nội dung PEM
            StringBuilder pemContent = new StringBuilder();
            pemContent.AppendLine(pemHeader);
            pemContent.AppendLine(base64Key);
            pemContent.AppendLine(pemFooter);

            // Ghi nội dung PEM vào file
            File.WriteAllText(pemFilePath, pemContent.ToString());
        }

        // Hàm đảm bảo độ dài chính xác của byte array
        byte[] AdjustByteArrayLength(byte[] byteArray, int length)
        {
            if (byteArray.Length == length) return byteArray;
            byte[] adjustedArray = new byte[length];
            Array.Copy(byteArray, 0, adjustedArray, length - byteArray.Length, byteArray.Length);
            return adjustedArray;
        }

        // Phương thức tạo chữ ký số (.pfx)
        public Org.BouncyCastle.Math.BigInteger ToCertificationFile(string name, Guid guid, string certFilePath)
        {
            // Khai báo các tham số khóa dưới dạng BigInteger
            BigInteger big = this._n.Equals(0) ? this._p * this._q : this._n;
            Org.BouncyCastle.Math.BigInteger n = new Org.BouncyCastle.Math.BigInteger(big.ToString());
            Org.BouncyCastle.Math.BigInteger e = Org.BouncyCastle.Math.BigInteger.ValueOf(65537);
            Org.BouncyCastle.Math.BigInteger d = new Org.BouncyCastle.Math.BigInteger(this._privateKey.ToString());

            Org.BouncyCastle.Math.BigInteger p = new Org.BouncyCastle.Math.BigInteger(this._p.ToString());
            Org.BouncyCastle.Math.BigInteger q = new Org.BouncyCastle.Math.BigInteger(this._q.ToString());
            Org.BouncyCastle.Math.BigInteger dp = d.Mod(p.Subtract(Org.BouncyCastle.Math.BigInteger.One));
            Org.BouncyCastle.Math.BigInteger dq = d.Mod(q.Subtract(Org.BouncyCastle.Math.BigInteger.One));
            Org.BouncyCastle.Math.BigInteger qInv = q.ModInverse(p);

            var calculatedN = p.Multiply(q);
            Console.WriteLine($"Does calculated n match provided n? {calculatedN.Equals(n)}");
            Console.WriteLine($"Is dp correct? {dp.Equals(d.Mod(p.Subtract(Org.BouncyCastle.Math.BigInteger.One)))}");
            Console.WriteLine($"Is dq correct? {dq.Equals(d.Mod(q.Subtract(Org.BouncyCastle.Math.BigInteger.One)))}");
            Console.WriteLine($"Is qInv correct? {qInv.Multiply(q).Mod(p).Equals(Org.BouncyCastle.Math.BigInteger.One)}");



            RsaPrivateCrtKeyParameters privateKeyParameters = new RsaPrivateCrtKeyParameters(
                n, e, d, p, q, dp, dq, qInv
            );
            // Độ dài của Modulus
            int modulusLength = n.ToByteArray().Length;
            int halfModulusLength = (modulusLength + 1) / 2;

            // Điều chỉnh các thành phần của RSAParameters
            RSAParameters rsaParameters = new RSAParameters
            {
                Modulus = n.ToByteArray(),
                Exponent = e.ToByteArray(),
                D = AdjustByteArrayLength(d.ToByteArray(), modulusLength),
                P = AdjustByteArrayLength(p.ToByteArray(), halfModulusLength),
                Q = AdjustByteArrayLength(q.ToByteArray(), halfModulusLength),
                DP = AdjustByteArrayLength(dp.ToByteArray(), halfModulusLength),
                DQ = AdjustByteArrayLength(dq.ToByteArray(), halfModulusLength),
                InverseQ = AdjustByteArrayLength(qInv.ToByteArray(), halfModulusLength)
            };
            // Chuyển đổi RsaPrivateCrtKeyParameters thành đối tượng RSA
            var rsaPrivateKey = RSA.Create();
            rsaPrivateKey.ImportParameters(rsaParameters);

            // Tạo đối tượng chứng chỉ
            X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();
            var certIssuer = new X509Name("CN=Công ty cổ phần SecureSign");
            var certName = new X509Name($"CN={name}");
            var guidBytes = guid.ToByteArray();
            var serialNumber = new Org.BouncyCastle.Math.BigInteger(guidBytes);
            certGen.SetSerialNumber(serialNumber.Abs());
            certGen.SetSubjectDN(certName);
            certGen.SetIssuerDN(certIssuer);
            certGen.SetNotBefore(DateTime.UtcNow);
            certGen.SetNotAfter(DateTime.UtcNow.AddYears(1));
            certGen.SetPublicKey(new RsaKeyParameters(false, n, e));

            // Ký chứng chỉ
            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA256WithRSA", privateKeyParameters);
            Org.BouncyCastle.X509.X509Certificate certificate = certGen.Generate(signatureFactory);

            // Tạo file PFX từ chứng chỉ và khóa
            X509Certificate2 x509Certificate = new X509Certificate2(DotNetUtilities.ToX509Certificate(certificate));
            // Sử dụng CopyWithPrivateKey để gán khóa riêng
            X509Certificate2 certificateWithPrivateKey = x509Certificate.CopyWithPrivateKey(rsaPrivateKey);

            // Xuất ra file .pfx
            var pfxBytes = certificateWithPrivateKey.Export(X509ContentType.Pfx, password: "dantruong");
            System.IO.File.WriteAllBytes(certFilePath, pfxBytes);

            return serialNumber;
        }
    }
}
