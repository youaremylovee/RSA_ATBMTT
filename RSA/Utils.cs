using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyRSA
{
	public class Utils
	{
		// Hàm Miller-Rabin thực hiện kiểm tra một lần
		private static bool MillerRabinTest(BigInteger d, BigInteger n)
		{
			Random rand = new Random();
			BigInteger a = 2 + rand.Next() % (n - 4); // Chọn số ngẫu nhiên a giữa 2 và n-2

			// Tính x = a^d % n
			BigInteger x = BigInteger.ModPow(a, d, n);

			if (x == 1 || x == n - 1)
				return true;

			// Tiếp tục kiểm tra d cho đến khi d == n-1
			while (d != n - 1)
			{
				x = (x * x) % n;
				d *= 2;

				if (x == 1) return false;
				if (x == n - 1) return true;
			}

			return false;
		}
		// Kiểm tra số nguyên tố bằng thuật toán Miller-Rabin
		public static bool IsPrimeMillerRabin(BigInteger n, int k)
		{
			if (n <= 1 || n == 4) return false;
			if (n <= 3) return true;

			BigInteger d = n - 1;
			while (d % 2 == 0)
				d /= 2;

			// Thực hiện kiểm tra k lần
			for (int i = 0; i < k; i++)
				if (!MillerRabinTest(d, n))
					return false;

			return true;
		}
		// Hàm tạo số BigInteger ngẫu nhiên trong khoảng [min, max]
		private static BigInteger RandomBigInteger(BigInteger min, BigInteger max, Random random)
		{
			BigInteger result;
			do
			{
				byte[] bytes = max.ToByteArray();
				random.NextBytes(bytes);
				bytes[bytes.Length - 1] &= 0x7F; // Đảm bảo số không âm
				result = new BigInteger(bytes);
			} while (result < min || result > max);
			return result;
		}
		// Hàm tạo số nguyên tố lớn có kích thước chữ số xác định
		public static BigInteger GenerateLargePrime(int digits, int k)
		{
			Random random = new Random();
			BigInteger prime;

			// Giới hạn khoảng số cần tạo
			BigInteger lowerBound = BigInteger.Pow(10, digits - 1); // Số có đúng (digits-1) chữ số
			BigInteger upperBound = BigInteger.Pow(10, digits) - 1; // Số lớn nhất có đúng (digits) chữ số

			do
			{
				// Tạo ra số ngẫu nhiên trong khoảng [lowerBound, upperBound]
				prime = RandomBigInteger(lowerBound, upperBound, random);

				// Đảm bảo số lẻ (vì số chẵn không thể là số nguyên tố trừ 2)
				if (prime % 2 == 0)
					prime += 1;

			} while (!IsPrimeMillerRabin(prime, k)); // Kiểm tra số nguyên tố bằng Miller-Rabin

			return prime;
		}
	}
}
