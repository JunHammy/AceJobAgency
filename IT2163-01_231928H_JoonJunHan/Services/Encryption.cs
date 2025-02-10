using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace IT2163_01_231928H_JoonJunHan.Services
{
    public class Encryption
    {
        private static readonly string Key;

        static Encryption()
        {
            // Build the configuration directly inside the class
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) // Use the current directory
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Retrieve the encryption key from appsettings.json
            Key = configuration["EncryptionSettings:EncryptionKey"]
                .Replace("\r", "").Replace("\n", "").PadRight(32, '0').Substring(0, 32); // Ensure 32 bytes
        }

        public string EncryptData(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(Key);
                aesAlg.GenerateIV(); // Generate a new IV for each encryption

                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

                    // Store IV + Encrypted data (Base64 encoded)
                    return Convert.ToBase64String(aesAlg.IV) + ":" + Convert.ToBase64String(encrypted);
                }
            }
        }

        public string DecryptData(string cipherText)
        {
            try
            {
                string[] parts = cipherText.Split(':');
                if (parts.Length != 2) throw new FormatException("Invalid encrypted text format");

                byte[] iv = Convert.FromBase64String(parts[0]);
                byte[] cipherBytes = Convert.FromBase64String(parts[1]);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(Key);
                    aesAlg.IV = iv;

                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    {
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Decryption failed.", ex);
            }
        }
    }
}
