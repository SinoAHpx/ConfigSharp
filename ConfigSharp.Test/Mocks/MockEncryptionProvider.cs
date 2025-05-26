using ConfigSharp.Interfaces;

namespace ConfigSharp.Test.Mocks;

/// <summary>
/// Mock encryption provider for testing purposes that performs simple base64 encoding/decoding.
/// </summary>
public class MockEncryptionProvider : IEncryptionProvider
{
    /// <summary>
    /// Encrypts plain text using simple base64 encoding (for testing only).
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <param name="password">The password (not used in this mock)</param>
    /// <returns>Base64 encoded text</returns>
    public string Encrypt(string plainText, string password)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));

        var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Decrypts cipher text using simple base64 decoding (for testing only).
    /// </summary>
    /// <param name="cipherText">The text to decrypt</param>
    /// <param name="password">The password (not used in this mock)</param>
    /// <returns>Base64 decoded text</returns>
    public string Decrypt(string cipherText, string password)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be null or empty", nameof(cipherText));

        var bytes = Convert.FromBase64String(cipherText);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Encrypts plain text asynchronously using simple base64 encoding (for testing only).
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <param name="password">The password (not used in this mock)</param>
    /// <returns>Base64 encoded text</returns>
    public async Task<string> EncryptAsync(string plainText, string password)
    {
        return await Task.Run(() => Encrypt(plainText, password));
    }

    /// <summary>
    /// Decrypts cipher text asynchronously using simple base64 decoding (for testing only).
    /// </summary>
    /// <param name="cipherText">The text to decrypt</param>
    /// <param name="password">The password (not used in this mock)</param>
    /// <returns>Base64 decoded text</returns>
    public async Task<string> DecryptAsync(string cipherText, string password)
    {
        return await Task.Run(() => Decrypt(cipherText, password));
    }
}