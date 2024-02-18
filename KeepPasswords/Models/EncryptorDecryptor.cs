using System.Security.Cryptography;
using System.Text;

namespace KeepPasswords.Models
{
    public static class EncryptorDecryptor
    {
        public static string EncryptPlainText(string key, string plainText)
        {
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, null);

                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                return Convert.ToBase64String(encryptedBytes);
            }
        }

        public static string DecryptToPlainText(string key, string encryptedText)
        {

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, null);

                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        public static byte[] EncryptBytes(byte[] imageData, byte[] key)
        {
            byte[] IV = new byte[] { 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = IV;

                using (MemoryStream msInput = new MemoryStream(imageData))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(msOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            msInput.CopyTo(cryptoStream);
                        }

                        return msOutput.ToArray();
                    }
                }
            }
        }

        public static byte[] DecryptBytes(byte[] imageData, byte[] key)
        {
            byte[] IV = new byte[] { 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 };
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = IV;

                using (MemoryStream msInput = new MemoryStream(imageData))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(msInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            cryptoStream.CopyTo(msOutput);
                        }

                        return msOutput.ToArray();
                    }
                }
            }
        }
    }
}

