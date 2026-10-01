using System.Security.Cryptography;
using System.Text;

namespace HCMPortal.Helper
{
    public class APIEncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456"); // 16-byte key
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("6543210987654321");  // 16-byte IV


        public static string Encrypt(string text)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(text);
                        cs.Write(inputBytes, 0, inputBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    // Return the full encrypted Base64 string (no truncation)
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));

            // Convert the Base64 cipher text back into a byte array
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream(cipherBytes))
                {
                    // Create a decryptor using the same Key and IV
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            // Read the decrypted bytes from the stream into a string
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }


    }
}
