using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace ProjectLighthouse.Helpers {
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class HashHelper {
//        private static readonly SHA1 sha1 = SHA1.Create();
        private static readonly SHA256 sha256 = SHA256.Create();
        private static readonly Random random = new();

        #region Hash Functions
        public static string Sha256Hash(string str) => Sha256Hash(Encoding.UTF8.GetBytes(str));
        
        public static string Sha256Hash(byte[] bytes) {
            byte[] hash = sha256.ComputeHash(bytes);
            return Encoding.UTF8.GetString(hash, 0, hash.Length);
        }

        public static string BCryptHash(string str) => BCrypt.Net.BCrypt.HashPassword(str);
        
        public static string BCryptHash(byte[] bytes) => BCrypt.Net.BCrypt.HashPassword(Encoding.UTF8.GetString(bytes));
        #endregion

        /// <summary>
        /// Generates a specified amount of random bytes in an array.
        /// </summary>
        /// <param name="count">The amount of bytes to generate.</param>
        /// <returns>The bytes generated</returns>
        public static IEnumerable<byte> GenerateRandomBytes(int count) {
            byte[] b = new byte[count];
            random.NextBytes(b);

            return b;
        }

        /// <summary>
        /// Generates a random SHA256 & BCrypted token 
        /// </summary>
        /// <returns>The token as a string.</returns>
        public static string GenerateAuthToken() {
            byte[] bytes = (byte[]) GenerateRandomBytes(256);

            return BCryptHash(Sha256Hash(bytes));
        }
    }
}