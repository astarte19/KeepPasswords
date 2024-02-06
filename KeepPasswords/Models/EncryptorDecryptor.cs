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

        public static byte[] EncryptBytes(byte[] plainBytes, string key)
        {
            byte[] encryptedBytes;
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.Key = Encoding.UTF8.GetBytes(key);
                rijndael.Mode = CipherMode.CBC;

                ICryptoTransform encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }

        public static byte[] DecryptBytes(byte[] encryptedBytes, string key)
        {
            byte[] decryptedBytes;
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.Key = Encoding.UTF8.GetBytes(key);
                rijndael.Mode = CipherMode.CBC;

                ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);

                using (MemoryStream ms = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream outputMs = new MemoryStream())
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = cs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                outputMs.Write(buffer, 0, bytesRead);
                            }
                            decryptedBytes = outputMs.ToArray();
                        }
                    }
                }
            }
            return decryptedBytes;
        }
    }
}
