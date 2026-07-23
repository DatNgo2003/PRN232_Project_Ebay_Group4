using System.Security.Cryptography;
using System.Text;

namespace Backend.Utils;

/// <summary>
/// So sánh 2 chuỗi bí mật (API key/secret) theo constant-time,
/// tránh timing attack khi verify secret key của webhook thanh toán.
/// </summary>
public static class ConstantTimeCompare
{
    public static bool Equals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);

        if (aBytes.Length != bBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}