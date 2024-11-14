using System.Security.Cryptography;
using System.Text;

namespace RSA_UI.Utils;

public static class StringUtils
{
    public static string ToMd5(this string input)
    {
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        StringBuilder sb = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
    public static bool IsNotNullOrEmpty(this string? input) => !string.IsNullOrEmpty(input);
}