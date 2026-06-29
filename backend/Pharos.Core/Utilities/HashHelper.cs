using System;
using System.Security.Cryptography;
using System.Text;

namespace Pharos.Core.Utilities
{
    public static class HashHelper
    {
        public static string ComputeSha256(string input)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        public static string ComputeSha256(byte[] input)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(input);
            return Convert.ToHexString(bytes);
        }

        public static string ComputeHmacSha256(string message, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}
