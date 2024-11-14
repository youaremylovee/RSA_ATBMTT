using System.Security.Cryptography;
using System.Text;

namespace Website.Utils
{
    public class Encryption
    {
        public static async Task<string?> GetFileHash(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            using (var sha256 = SHA256.Create())
            using (var stream = file.OpenReadStream())
            {
                byte[] hashBytes = await sha256.ComputeHashAsync(stream);

                // Chuyển đổi mảng byte thành chuỗi Hex
                StringBuilder hashStringBuilder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashStringBuilder.Append(b.ToString("x2"));
                }

                return hashStringBuilder.ToString(); // Trả về giá trị băm dưới dạng chuỗi Hex
            }
        }
    }
}
