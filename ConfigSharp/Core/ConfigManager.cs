using ConfigSharp.Encryption;
using ConfigSharp.Exceptions;
using ConfigSharp.Interfaces;

namespace ConfigSharp.Core;

/// <summary>
/// Main configuration manager that orchestrates loading and saving configurations with optional encryption.
/// </summary>
public class ConfigManager<TProvider, TEncryption>
    where TProvider : IConfigProvider
    where TEncryption : IEncryptionProvider
{
    private readonly TProvider _configProvider = Activator.CreateInstance<TProvider>();
    private readonly TEncryption _encryptionProvider = Activator.CreateInstance<TEncryption>();

    /// <summary>
    /// Loads configuration of the specified type from the given file path asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration to load</typeparam>
    /// <param name="filePath">The path to the configuration file</param>
    /// <param name="password">Optional password for encrypted configurations</param>
    /// <returns>The loaded configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be loaded</exception>
    /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    public async Task<T> LoadConfigAsync<T>(string filePath, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        try
        {
            if (!string.IsNullOrEmpty(password))
            {
                var encryptedContent = await _configProvider.LoadAsStringAsync(filePath);
                var decryptedContent = await _encryptionProvider.DecryptAsync(encryptedContent, password!);
                return _configProvider.Deserialize<T>(decryptedContent);
            }

            return await _configProvider.LoadAsync<T>(filePath);
        }
        catch (Exception ex) when (!(ex is ConfigReadException) && !(ex is EncryptionException) && !(ex is ArgumentException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while loading configuration", ex);
        }
    }

    /// <summary>
    /// Saves the specified configuration to the given file path asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration to save</typeparam>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="configData">The configuration object to save</param>
    /// <param name="password">Optional password to encrypt the configuration</param>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be saved</exception>
    /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if configData is null.</exception>
    public async Task SaveConfigAsync<T>(string filePath, T configData, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }
        if (configData == null)
        {
            throw new ArgumentNullException(nameof(configData), "Configuration data cannot be null");
        }

        try
        {
            if (!string.IsNullOrEmpty(password))
            {
                var serializedData = _configProvider.Serialize(configData);
                var encryptedContent = await _encryptionProvider.EncryptAsync(serializedData, password!);
                await _configProvider.SaveAsStringAsync(filePath, encryptedContent);
            }
            else
            {
                await _configProvider.SaveAsync(filePath, configData);
            }
        }
        catch (Exception ex) when (!(ex is ConfigReadException) && !(ex is EncryptionException) && !(ex is ArgumentException) && !(ex is ArgumentNullException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving configuration", ex);
        }
    }

    /// <summary>
    /// Loads configuration of the specified type from the given file path synchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration to load</typeparam>
    /// <param name="filePath">The path to the configuration file</param>
    /// <param name="password">Optional password for encrypted configurations</param>
    /// <returns>The loaded configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be loaded</exception>
    /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    public T LoadConfig<T>(string filePath, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        try
        {
            if (!string.IsNullOrEmpty(password))
            {
                var encryptedContent = _configProvider.LoadAsString(filePath);
                var decryptedContent = _encryptionProvider.Decrypt(encryptedContent, password!);
                return _configProvider.Deserialize<T>(decryptedContent);
            }
            else
            {
                return _configProvider.Load<T>(filePath);
            }
        }
        catch (Exception ex) when (!(ex is ConfigReadException) && !(ex is EncryptionException) && !(ex is ArgumentException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while loading configuration", ex);
        }
    }

    /// <summary>
    /// Saves the specified configuration to the given file path synchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration to save</typeparam>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="configData">The configuration object to save</param>
    /// <param name="password">Optional password to encrypt the configuration</param>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be saved</exception>
    /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if configData is null.</exception>
    public void SaveConfig<T>(string filePath, T configData, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }
        if (configData == null)
        {
            throw new ArgumentNullException(nameof(configData), "Configuration data cannot be null");
        }

        try
        {
            if (!string.IsNullOrEmpty(password))
            {
                var serializedData = _configProvider.Serialize(configData);
                var encryptedContent = _encryptionProvider.Encrypt(serializedData, password!);
                _configProvider.SaveAsString(filePath, encryptedContent);
            }
            else
            {
                _configProvider.Save(filePath, configData);
            }
        }
        catch (Exception ex) when (!(ex is ConfigReadException) && !(ex is EncryptionException) && !(ex is ArgumentException) && !(ex is ArgumentNullException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving configuration", ex);
        }
    }
}