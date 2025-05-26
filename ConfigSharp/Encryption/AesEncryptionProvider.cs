using System.Security.Cryptography;
using System.Text;
using ConfigSharp.Exceptions;
using ConfigSharp.Interfaces;

namespace ConfigSharp.Encryption;

/// <summary>
/// AES encryption provider for encrypting and decrypting configuration data.
/// </summary>
public class AesEncryptionProvider : IEncryptionProvider
{
    private const int KeySize = 256; // 256-bit key
    private const int IvSize = 16;   // 128-bit IV
    private const int SaltSize = 16; // 128-bit salt
    private const int Iterations = 10000; // PBKDF2 iterations

    /// <summary>
    /// Encrypts the specified plain text using AES encryption with the provided password.
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <param name="password">The password to use for encryption</param>
    /// <returns>The encrypted text as a Base64 string</returns>
    /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
    public string Encrypt(string plainText, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            // Generate random salt and IV
            var salt = new byte[SaltSize];
            var iv = new byte[IvSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
                rng.GetBytes(iv);
            }

            // Derive key from password
            var key = DeriveKeyFromPassword(password, salt);

            // Encrypt the data
            byte[] encryptedBytes;
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }

            // Combine salt + IV + encrypted data
            var result = new byte[SaltSize + IvSize + encryptedBytes.Length];
            Array.Copy(salt, 0, result, 0, SaltSize);
            Array.Copy(iv, 0, result, SaltSize, IvSize);
            Array.Copy(encryptedBytes, 0, result, SaltSize + IvSize, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            throw new EncryptionException("encryption", "Failed to encrypt data", ex);
        }
    }

    /// <summary>
    /// Decrypts the specified cipher text using AES decryption with the provided password.
    /// </summary>
    /// <param name="cipherText">The encrypted text as a Base64 string</param>
    /// <param name="password">The password to use for decryption</param>
    /// <returns>The decrypted plain text</returns>
    /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
    public string Decrypt(string cipherText, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                throw new ArgumentException("Cipher text cannot be null or empty", nameof(cipherText));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            // Decode from Base64
            var encryptedData = Convert.FromBase64String(cipherText);

            if (encryptedData.Length < SaltSize + IvSize)
            {
                throw new EncryptionException("decryption", "Invalid encrypted data format");
            }

            // Extract salt, IV, and encrypted bytes
            var salt = new byte[SaltSize];
            var iv = new byte[IvSize];
            var encryptedBytes = new byte[encryptedData.Length - SaltSize - IvSize];

            Array.Copy(encryptedData, 0, salt, 0, SaltSize);
            Array.Copy(encryptedData, SaltSize, iv, 0, IvSize);
            Array.Copy(encryptedData, SaltSize + IvSize, encryptedBytes, 0, encryptedBytes.Length);

            // Derive key from password
            var key = DeriveKeyFromPassword(password, salt);

            // Decrypt the data
            byte[] decryptedBytes;
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex) when (!(ex is ArgumentException) && !(ex is EncryptionException))
        {
            throw new EncryptionException("decryption", "Failed to decrypt data", ex);
        }
    }

    /// <summary>
    /// Encrypts the specified plain text using AES encryption with the provided password asynchronously.
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <param name="password">The password to use for encryption</param>
    /// <returns>The encrypted text as a Base64 string</returns>
    /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
    public async Task<string> EncryptAsync(string plainText, string password)
    {
        return await Task.Run(() => Encrypt(plainText, password));
    }

    /// <summary>
    /// Decrypts the specified cipher text using AES decryption with the provided password asynchronously.
    /// </summary>
    /// <param name="cipherText">The encrypted text as a Base64 string</param>
    /// <param name="password">The password to use for decryption</param>
    /// <returns>The decrypted plain text</returns>
    /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
    public async Task<string> DecryptAsync(string cipherText, string password)
    {
        return await Task.Run(() => Decrypt(cipherText, password));
    }

    /// <summary>
    /// Derives a cryptographic key from a password and salt using PBKDF2.
    /// </summary>
    /// <param name="password">The password to derive the key from</param>
    /// <param name="salt">The salt to use in key derivation</param>
    /// <returns>The derived key</returns>
    private static byte[] DeriveKeyFromPassword(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(KeySize / 8); // Convert bits to bytes
    }
}