using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BAL
{
    public class AESEncrytDecry
    {
        public static string DecryptStringFromBytes(byte[] cipherText, string secreatkey, byte[] iv)
        {
            // Check arguments.  
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (secreatkey == null || secreatkey.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold  
            // the decrypted text.  
            string plaintext = null;

            // Create an RijndaelManaged object  
            // with the specified key and IV.  
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.IV = iv;
                string key = secreatkey.ToString();
                byte[] key1 = Encoding.UTF8.GetBytes(key);
                rijAlg.Key = key1;
                // Create a decrytor to perform the stream transform.  
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    // Create the streams used for decryption.  
                    using (var msDecrypt = new MemoryStream(cipherText))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {

                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream  
                                // and place them in a string.  
                                plaintext = srDecrypt.ReadToEnd();

                            }

                        }
                    }
                }
                catch
                {
                    plaintext = "keyError";
                }
            }

            return plaintext;

        }

        public static string DecryptStringAES(string cipherText)
        {



            string guied = cipherText.Substring(cipherText.Length - 16, 16);
            string emIdEncrypt = cipherText.Substring(0, cipherText.Length - 16);
            //var keybytes = Encoding.UTF8.GetBytes(guied);
            string secreatkey = "$P@mOu$0172@0r!P";

            var iv = Encoding.UTF8.GetBytes(guied);

            var encrypted = Convert.FromBase64String(emIdEncrypt);
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, secreatkey, iv);
            return decriptedFromJavascript;
        }
      
        public static string DecryptStringAES(string cipherText, string Tokenguied)
        {
            string guied = Tokenguied.Substring(0, 16);
            //var secreatkey = Encoding.UTF8.GetBytes(guied);
            //string secreatkey = "d7a50e0f2f9546d35ce700eebfb0c911";
            string secreatkey = "$P@mOu$0172@0r!P";

            var iv = Encoding.UTF8.GetBytes(guied);
           // string iv = "@1O2j3D4e5F6g7P8";

            //string iv = "lw-hv-ThGioHnTAi";
            var encrypted = Convert.FromBase64String(cipherText);
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, secreatkey, iv);
            return string.Format(decriptedFromJavascript);
        }

        public static string DecryptString(string input, string key)
        {
            byte[] inputArray = Convert.FromBase64String(input);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }


        static readonly char[] CharacterMatrixForRandomIVStringGeneration = {

                     'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',

                     'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',

                     'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',

                     'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',

                     '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'

              };

        internal static string GenerateRandomIV(int length)

        {
            char[] _iv = new char[length];

            byte[] randomBytes = new byte[length];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())

            {
                rng.GetBytes(randomBytes); //Fills an array of bytes with a cryptographically strong sequence of random values.
            }
            for (int i = 0; i < _iv.Length; i++)

            {
                int ptr = randomBytes[i] % CharacterMatrixForRandomIVStringGeneration.Length;

                _iv[i] = CharacterMatrixForRandomIVStringGeneration[ptr];
            }
            return new string(_iv);
        }

        public static string Encryptstring(string Data)
        {
            string Input = Data;
            string SecreatKey = "$P@mOu$0172@0r!P";
            var ivkeytest = GenerateRandomIV(16);
            // string ivkeytest = "lw-hv-ThGioHnTAi";               
            var EncryptData = EncryptString(Input, SecreatKey, ivkeytest);

            var Concate = EncryptData + ivkeytest;
            return Concate;
        }

        public static string EncryptString(string plainInput, string SecreatKey, string IV)
        {
            byte[] iv = Encoding.UTF8.GetBytes(IV);
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(SecreatKey);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainInput);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }

    }
}
