using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse
{
    public static class DigestUtils
    {
        public static async Task<string> ComputeDigest(string path, string authCookie, Stream body,
            string digestKey)
        {
            var memoryStream = new MemoryStream();

            var pathBytes = Encoding.UTF8.GetBytes(path);
            var cookieBytes = string.IsNullOrEmpty(authCookie)
                ? Array.Empty<byte>()
                : Encoding.UTF8.GetBytes(authCookie);
            var keyBytes = Encoding.UTF8.GetBytes(digestKey);
                
            await body.CopyToAsync(memoryStream);

            var bodyBytes = memoryStream.ToArray();

            using var sha1 = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
            sha1.AppendData(bodyBytes);
            if (cookieBytes.Length > 0)
                sha1.AppendData(cookieBytes);
            sha1.AppendData(pathBytes);
            sha1.AppendData(keyBytes);

            var digestBytes = sha1.GetHashAndReset();
            var digestString = Convert.ToHexString(digestBytes).ToLower();

            return digestString;
        }
    }
}