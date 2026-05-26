using System.Security.Cryptography;
using System.Text;

namespace Jarvis.Domain.Shared.Utility;

#nullable disable

/// <summary>
/// Helper class provides functions for Hashing, Encrypt/Descrypt
/// </summary>
public static class CryptographyHelper
{
    /// <summary>
    /// Hash data from byte[]
    /// </summary>
    /// <param name="alg">Hash algorithm name. Ex: MD5, SHA1, SHA256, SHA384, SHA512</param>
    /// <param name="data">Data for hashing</param>
    /// <returns></returns>
#pragma warning disable SYSLIB0045 // Type or member is obsolete
    public static byte[] GetHash(HashAlgorithmName alg, byte[] data) => HashAlgorithm.Create(alg.Name).ComputeHash(data);
#pragma warning restore SYSLIB0045 // Type or member is obsolete


    /// <summary>
    /// Hash data from string
    /// </summary>
    /// <param name="alg">Hash algorithm name. Ex: MD5, SHA1, SHA256, SHA384, SHA512</param>
    /// <param name="data">Data for hashing</param>
    /// <returns></returns>
    public static byte[] GetHash(HashAlgorithmName alg, string data) => GetHash(alg, Encoding.UTF8.GetBytes(data));

    public static byte[] HMACSHA256(string key, string message) => HMACSHA256(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(message));

    public static byte[] HMACSHA256(byte[] key, byte[] message) => new HMACSHA256(key).ComputeHash(message);
}
