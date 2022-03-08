using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Helpers;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class HashHelper
{
    // private static readonly SHA1 sha1 = SHA1.Create();
    private static readonly SHA256 sha256 = SHA256.Create();

    /// <summary>
    ///     Generates a specified amount of random bytes in an array.
    /// </summary>
    /// <param name="count">The amount of bytes to generate.</param>
    /// <returns>The bytes generated</returns>
    public static IEnumerable<byte> GenerateRandomBytes(int count)
    {
        byte[] b = new byte[count];
        
        lock (RandomHelper.random)
        {
            RandomHelper.random.NextBytes(b);
        }
        
        return b;
    }

    /// <summary>
    ///     Generates a random SHA256 & BCrypted token
    /// </summary>
    /// <returns>The token as a string.</returns>
    public static string GenerateAuthToken()
    {
        byte[] bytes = (byte[])GenerateRandomBytes(256);

        return BCryptHash(Sha256Hash(bytes));
    }

    public static async Task<string> ComputeDigest(string path, string authCookie, Stream body, string digestKey)
    {
        MemoryStream memoryStream = new();

        byte[] pathBytes = Encoding.UTF8.GetBytes(path);
        byte[] cookieBytes = string.IsNullOrEmpty(authCookie) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(authCookie);
        byte[] keyBytes = Encoding.UTF8.GetBytes(digestKey);

        await body.CopyToAsync(memoryStream);

        byte[] bodyBytes = memoryStream.ToArray();

        using IncrementalHash sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        sha1.AppendData(bodyBytes);
        if (cookieBytes.Length > 0) sha1.AppendData(cookieBytes);
        sha1.AppendData(pathBytes);
        sha1.AppendData(keyBytes);

        byte[] digestBytes = sha1.GetHashAndReset();
        string digestString = Convert.ToHexString(digestBytes).ToLower();

        return digestString;
    }

    #region Hash Functions

    public static string Sha256Hash(string str) => Sha256Hash(Encoding.UTF8.GetBytes(str));

    public static string Sha256Hash(byte[] bytes) => BitConverter.ToString(sha256.ComputeHash(bytes)).Replace("-", "").ToLower();

    public static string Sha1Hash(string str) => Sha1Hash(Encoding.UTF8.GetBytes(str));

    public static string Sha1Hash(byte[] bytes) => BitConverter.ToString(SHA1.Create().ComputeHash(bytes)).Replace("-","");

    public static string BCryptHash(string str) => BCrypt.Net.BCrypt.HashPassword(str);

    public static string BCryptHash(byte[] bytes) => BCrypt.Net.BCrypt.HashPassword(Encoding.UTF8.GetString(bytes));

    #endregion

}