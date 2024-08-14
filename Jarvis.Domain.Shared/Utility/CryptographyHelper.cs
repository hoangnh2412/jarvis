using System.Security.Cryptography;
using System.Text;

namespace Jarvis.Domain.Shared.Utility;

#nullable disable

/// <summary>
/// Helper class provides functions for Hashing, Encrypt/Descrypt
/// </summary>
public static partial class CryptographyHelper
{
    /// <summary>
    /// Hash data from byte[]
    /// </summary>
    /// <param name="alg">Hash algorithm name. Ex: MD5, SHA1, SHA256, SHA384, SHA512</param>
    /// <param name="data">Data for hashing</param>
    /// <returns></returns>
    public static byte[] GetHash(HashAlgorithmName alg, byte[] data)
    {
#pragma warning disable SYSLIB0045 // Type or member is obsolete
        return HashAlgorithm.Create(alg.Name).ComputeHash(data);
#pragma warning restore SYSLIB0045 // Type or member is obsolete
    }

    /// <summary>
    /// Hash data from string
    /// </summary>
    /// <param name="alg">Hash algorithm name. Ex: MD5, SHA1, SHA256, SHA384, SHA512</param>
    /// <param name="data">Data for hashing</param>
    /// <returns></returns>
    public static byte[] GetHash(HashAlgorithmName alg, string data)
    {
        return GetHash(alg, Encoding.UTF8.GetBytes(data));
    }
}
