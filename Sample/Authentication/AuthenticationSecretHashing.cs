using System.Security.Cryptography;
using System.Text;

namespace Sample.Authentication;

/// <summary>Hash / verify secret cho Sample storage-backed auth (không lưu plaintext trong DB).</summary>
internal static class AuthenticationSecretHashing
{
    private static readonly byte[] BasicPasswordSalt = "Sample.BasicAuth.v1"u8.ToArray();

    public static string HashApiKey(string secret)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyApiKey(string secret, string storedHashBase64)
    {
        if (string.IsNullOrEmpty(storedHashBase64))
            return false;

        try
        {
            var expected = Convert.FromBase64String(storedHashBase64);
            var actual = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
            return expected.Length == actual.Length
                && CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string HashBasicPassword(string password)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            BasicPasswordSalt,
            100_000,
            HashAlgorithmName.SHA256,
            32);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyBasicPassword(string password, string storedHashBase64)
    {
        if (string.IsNullOrEmpty(storedHashBase64))
            return false;

        try
        {
            var expected = Convert.FromBase64String(storedHashBase64);
            var actual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                BasicPasswordSalt,
                100_000,
                HashAlgorithmName.SHA256,
                32);
            return expected.Length == actual.Length
                && CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
