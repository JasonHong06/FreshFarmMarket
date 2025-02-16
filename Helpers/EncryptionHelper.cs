using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FreshFarmMarket.Helpers
{
    public static class EncryptionHelper
    {
        private static string _encryptionKey;

        public static void Configure(IConfiguration configuration)
        {
            _encryptionKey = configuration["EncryptionKey"];
        }

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            ms.Write(aes.IV);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            var iv = new byte[aes.IV.Length];
            Array.Copy(fullCipher, iv, iv.Length);
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);

            using var decryptor = aes.CreateDecryptor(aes.Key, iv);
            using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
