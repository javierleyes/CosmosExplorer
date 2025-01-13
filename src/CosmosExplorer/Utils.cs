// <copyright file="Utils.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CosmosExplorer.Core
{
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Utility class containing helper methods.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Generates a SHA-256 hash for the given input string.
        /// </summary>
        /// <param name="input">The input string to hash.</param>
        /// <returns>A hexadecimal string representation of the SHA-256 hash.</returns>
        public static string GenerateHash(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert the byte array to a hexadecimal string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Encrypts the specified text using AES encryption.
        /// </summary>
        /// <param name="input">The input to encrypt.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="iv">The initialization vector.</param>
        /// <returns>The encrypted text as a base64 encoded string.</returns>
        public static string Encrypt(string input, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key.Take(32).ToArray();
            aes.IV = iv.Take(16).ToArray();

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using StreamWriter sw = new StreamWriter(cs);
                sw.Write(input);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Decrypts the specified encrypted text using AES decryption.
        /// </summary>
        /// <param name="encryptedInput">The encrypted input as a base64 encoded string.</param>
        /// <param name="key">The decryption key.</param>
        /// <param name="iv">The initialization vector.</param>
        /// <returns>The decrypted text.</returns>
        public static string Decrypt(string encryptedInput, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key.Take(32).ToArray();
            aes.IV = iv.Take(16).ToArray();

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream ms = new MemoryStream(Convert.FromBase64String(encryptedInput));
            using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
