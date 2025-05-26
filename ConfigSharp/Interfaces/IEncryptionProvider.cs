namespace ConfigSharp.Interfaces;

/// <summary>
/// Interface for encryption providers that can encrypt and decrypt data.
/// </summary>
public interface IEncryptionProvider
{
    /// <summary>
    /// Encrypts the specified plain text using the provided password.
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <param name="password">The password to use for encryption</param>
    /// <returns>The encrypted text</returns>
    string Encrypt(string plainText, string password);

    /// <summary>
    /// Decrypts the specified cipher text using the provided password.
    /// </summary>
    /// <param name="cipherText">The encrypted text to decrypt</param>
    /// <param name="password">The password to use for decryption</param>
    /// <returns>The decrypted plain text</returns>
    string Decrypt(string cipherText, string password);

    /// <summary>
    /// Encrypts the specified plain text using the provided password asynchronously.
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <param name="password">The password to use for encryption</param>
    /// <returns>The encrypted text</returns>
    Task<string> EncryptAsync(string plainText, string password);

    /// <summary>
    /// Decrypts the specified cipher text using the provided password asynchronously.
    /// </summary>
    /// <param name="cipherText">The encrypted text to decrypt</param>
    /// <param name="password">The password to use for decryption</param>
    /// <returns>The decrypted plain text</returns>
    Task<string> DecryptAsync(string cipherText, string password);
}