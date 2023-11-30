using DSA.CreditCardProcess.Web.Services.Contracts;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DSA.CreditCardProcess.Web.Services
{
    public class CardProcessService : ICardProcessService
    {
        List<string> result = new List<string>();
        public async Task<List<string>> ProcessCard(string cardNumber)
        {
            result = new List<string>();
            try
            {
                IsCardNumberValid(cardNumber);
                
                result.Add("Generando salt y llaves aleatorias");
                //Generando llaves aleatorias para simulacion de este ejercicio, recordemos que la llave se debe guardar usando alguna tecnología segura
                
                byte[] key = new byte[32]; // 256 bit
                byte[] iv = new byte[16]; // 128 bit
                byte[] salt = new byte[8]; // 64 bit

                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(key);
                    rng.GetBytes(iv);
                    rng.GetBytes(salt);
                }               

                result.Add($"Key(256 bit): {Convert.ToBase64String(key)} ");
                result.Add($"IV(128 bit): {Convert.ToBase64String(iv)} ");
                result.Add($"Salt(64 bit): {Convert.ToBase64String(salt)} ");

                var shaValuePartA = sha256_hash(cardNumber, salt);
                result.Add("Primer Hash desde dato original: " + shaValuePartA);

                // Encriptacion
                byte[] ciphertext = Encrypt(cardNumber, key, iv);
                string encryptedText = Convert.ToBase64String(ciphertext);
                result.Add("Texto encriptado: " + encryptedText);
                //TODO proceso de insert en base de datos de encryptedText
                //TODO proceso de select en base de datos de encryptedText
                // Desencriptacion
                byte[] bytes = Convert.FromBase64String(encryptedText);
                string decryptedText = Decrypt(bytes, key, iv);
                result.Add("texto desencriptado: " + decryptedText);

                var shaValuePartB = sha256_hash(decryptedText, salt);
                result.Add("Segundo Hash desde desencriptacion : " + shaValuePartB);

                if (shaValuePartA != shaValuePartB)
                    throw new ArgumentException("Fallo proceso, los hash no coinciden.");

                result.Add("Proceso Exitoso, los hash coinciden");
                return result;
            }
            catch (Exception ex)
            {
                result.Add(ex.Message);
                return result;
            }
            finally
            {
                result.Add("Proceso finalizado");                
            }
        }
        
        private string sha256_hash(string cardNumber, byte[] salt)
        {
            StringBuilder Sb = new StringBuilder();

            using (var hash = new HMACSHA256(salt))
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(cardNumber));
                
                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        private void IsCardNumberValid(string cardNumber)
        {
            result.Add("Validando número de tarjeta -Regex-");
            var regex = @"^[0-9]{16}$";
            
            if(!Regex.IsMatch(cardNumber, regex, RegexOptions.IgnoreCase))
                throw new ArgumentException("Invalid card number");
            
            result.Add("Número de tarjeta correcto");

        }

        public static byte[] Encrypt(string plaintext, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                byte[] encryptedBytes;
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
                        csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                    }
                    encryptedBytes = msEncrypt.ToArray();
                }
                return encryptedBytes;
            }
        }
        public static string Decrypt(byte[] ciphertext, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                byte[] decryptedBytes;
                using (var msDecrypt = new MemoryStream(ciphertext))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var msPlain = new MemoryStream())
                        {
                            csDecrypt.CopyTo(msPlain);
                            decryptedBytes = msPlain.ToArray();
                        }
                    }
                }
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
