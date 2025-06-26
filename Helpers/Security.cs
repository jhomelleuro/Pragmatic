using System.Security.Cryptography;
using System.Text;

namespace Pragmatic.Helpers
{
    internal static class Security
    {
        public static string DecryptFromBase64(string encryptedText)
        {
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

                using SymmetricAlgorithm crypt = Aes.Create();
                crypt.Key = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes("gumIn7HX;sNDy6mfm/R="));
                crypt.IV = new byte[16];

                using MemoryStream memoryStream = new(encryptedBytes);
                using CryptoStream cryptoStream = new(memoryStream, crypt.CreateDecryptor(), CryptoStreamMode.Read);
                using MemoryStream decryptedMemoryStream = new();
                cryptoStream.CopyTo(decryptedMemoryStream);
                byte[] decryptedBytes = decryptedMemoryStream.ToArray();

                byte[] userHash = new byte[16];
                Array.Copy(decryptedBytes, decryptedBytes.Length - 16, userHash, 0, 16);

                byte[] computedHash = MD5.Create().ComputeHash(decryptedBytes, 0, decryptedBytes.Length - 16);
                if (!userHash.SequenceEqual(computedHash))
                {
                    throw new Exception("Invalid Hash");
                }

                return Encoding.UTF8.GetString(decryptedBytes, 0, decryptedBytes.Length - 16);

            }
            catch (Exception ex)
            {
                throw new Exception("Decryption failed", ex);
            }
        }

        public static string EncryptToBase64(string originalText)
        {
            try
            {
                byte[] userBytes = Encoding.UTF8.GetBytes(originalText);
                byte[] userHash = MD5.HashData(userBytes);
                byte[] key = MD5.HashData(Encoding.UTF8.GetBytes("gumIn7HX;sNDy6mfm/R="));
                byte[] iv = new byte[16];

                using SymmetricAlgorithm crypt = Aes.Create();
                crypt.Key = key;
                crypt.IV = iv;

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(userBytes, 0, userBytes.Length);
                cryptoStream.Write(userHash, 0, userHash.Length);
                cryptoStream.FlushFinalBlock();

                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception("Encryption failed", ex);
            }
        }
    }
}
