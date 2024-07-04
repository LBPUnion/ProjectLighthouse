using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LBPUnion.ProjectLighthouse.Extensions;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class CryptoHelper
{
    /// <summary>
    ///     Generates a random SHA256 and BCrypted token
    /// </summary>
    /// <returns>The token as a string.</returns>
    public static string GenerateAuthToken()
    {
        byte[] bytes = (byte[])GenerateRandomBytes(256);
        return BCryptHash(Sha256Hash(bytes));
    }

    public static string GenerateUrlToken()
    {
        byte[] bytes = (byte[])GenerateRandomBytes(256);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(BCryptHash(Sha256Hash(bytes))));
    }

    public static string ComputeDigest(string path, string authCookie, byte[] body, string digestKey, bool excludeBody = false)
    {

        byte[] pathBytes = Encoding.UTF8.GetBytes(path);
        byte[] cookieBytes = string.IsNullOrEmpty(authCookie) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(authCookie);
        byte[] keyBytes = Encoding.UTF8.GetBytes(digestKey);

        using IncrementalHash sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        // LBP games will sometimes opt to calculate the digest without the body
        // (one example is resource upload requests)
        if (!excludeBody)
            sha1.AppendData(body);
        if (cookieBytes.Length > 0) sha1.AppendData(cookieBytes);
        sha1.AppendData(pathBytes);
        sha1.AppendData(keyBytes);

        byte[] digestBytes = sha1.GetHashAndReset();
        string digestString = Convert.ToHexString(digestBytes).ToLower();

        return digestString;
    }

    /// <summary>
    ///     Generates a specified amount of random bytes in an array.
    /// </summary>
    /// <param name="count">The amount of bytes to generate.</param>
    /// <returns>The bytes generated</returns>
    public static IEnumerable<byte> GenerateRandomBytes(int count) => RandomNumberGenerator.GetBytes(count);

    /// <summary>
    ///     Generates a random bounded 32 bit integer
    /// </summary>
    /// <param name="fromInclusive">The lowest possible integer than can be generated, inclusive</param>
    /// <param name="toExclusive">The highest possible integer than can be generated, exclusive</param>
    /// <returns>The randomly generated integer</returns>
    public static int GenerateRandomInt32(int fromInclusive, int toExclusive) => RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);

    /// <summary>
    /// Generates a random 32 bit integer between 0 and 2^32 - 1
    /// </summary>
    /// <returns>The randomly generated integer</returns>
    public static int GenerateRandomInt32() => RandomNumberGenerator.GetInt32(0, int.MaxValue);

    public static string ToBase64(string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        return Convert.ToBase64String(bytes);
    }

    public static string FromBase64(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    #region Two Factor Authentication

    public static string GenerateTotpSecret()
    {
        // RFC 4226 recommends the secret to be 160 bits i.e. 20 bytes
        byte[] rand = (byte[])GenerateRandomBytes(20);

        // Base 64 bad apparently
        return Base32Encoding.ToString(rand);
    }

    public static bool VerifyBackup(string code, string backups) => backups.Split(",").Any(backup => ValuesEqual(code, backup));

    public static bool VerifyCode(string code, string secret)
    {
        if (code.Length != 6) return false;
        
        long window = TimeHelper.Timestamp / 30;

        byte[] secretBytes = Base32Encoding.ToBytes(secret);
        for (int i = -1; i <= 1; i++)
        {
            byte[] windowBytes = BitConverter.GetBytes(window + i);
            if (BitConverter.IsLittleEndian) windowBytes.Reverse();

            long genCode = generateTotpCode(secretBytes, windowBytes);
            string strCode = genCode.ToString();
            strCode = strCode.Substring(strCode.Length - 6, 6);
            if (ValuesEqual(strCode, code))
            {
                return true;
            }
        }
        return false;
    }

    private static long generateTotpCode(byte[] secret, byte[] data)
    {
        using HMACSHA1 hmac = new(secret);

        byte[] computedHash = hmac.ComputeHash(data);

        // The RFC has a hard coded index 19 in this value.
        // This is the same thing but also accommodates SHA256 and SHA512
        // hmacComputedHash[19] => hmacComputedHash[hmacComputedHash.Length - 1]

        int offset = computedHash[^1] & 0xf;
        return (computedHash[offset] & 0x7f) << 24 |
               (computedHash[offset + 1] & 0xff) << 16 |
               (computedHash[offset + 2] & 0xff) << 8 |
               (computedHash[offset + 3] & 0xff) % 1000000;
    }

    // Constant time comparison of two values
    private static bool ValuesEqual(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }

    public static string GenerateTotpLink(string secret, string issuer, string username) => $"otpauth://totp/{issuer}:{username}?secret={secret}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";

    #endregion

    #region Hash Functions

    public static string Sha256Hash(string str) => Sha256Hash(Encoding.UTF8.GetBytes(str));

    public static string Sha256Hash(byte[] bytes) => BitConverter.ToString(SHA256.HashData(bytes)).Replace("-", "").ToLower();

    public static string Sha1Hash(byte[] bytes) => BitConverter.ToString(SHA1.HashData(bytes)).Replace("-", "");

    public static string BCryptHash(string str) => BCrypt.Net.BCrypt.HashPassword(str);

    #endregion
}