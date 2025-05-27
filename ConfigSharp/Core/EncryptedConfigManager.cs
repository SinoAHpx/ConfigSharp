using ConfigSharp.Encryption;
using ConfigSharp.Exceptions;
using ConfigSharp.Interfaces;

namespace ConfigSharp.Core;

/// <summary>
/// Configuration manager that orchestrates loading and saving configurations with encryption support.
/// </summary>
public class EncryptedConfigManager<TProvider, TEncryption> : IConfigManager
    where TProvider : IConfigProvider
    where TEncryption : IEncryptionProvider
{
    public TProvider ConfigProvider { get; set; } = Activator.CreateInstance<TProvider>();
    public TEncryption EncryptionProvider { get; set; } = Activator.CreateInstance<TEncryption>();
    public string Password { get; set; }

    public EncryptedConfigManager(string password, TProvider? configProvider = default, TEncryption? encryptionProvider = default)
    {
        Password = password ?? throw new ArgumentNullException(nameof(password), "Password cannot be null");

        if (configProvider != null)
        {
            ConfigProvider = configProvider;
        }
        if (encryptionProvider != null)
        {
            EncryptionProvider = encryptionProvider;
        }
    }

    /// <summary>
    /// Loads configuration of the specified type from the given file path asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration to load</typeparam>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The loaded configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be loaded</exception>
    /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    public async Task<T> LoadAsync<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        try
        {
            var encryptedContent = await ConfigProvider.LoadAsStringAsync(filePath);
            var decryptedContent = await EncryptionProvider.DecryptAsync(encryptedContent, Password);
            return ConfigProvider.Deserialize<T>(decryptedContent);
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not EncryptionException && ex is not ArgumentException)
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
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be saved</exception>
    /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if configData is null.</exception>
    public async Task SaveAsync<T>(string filePath, T configData)
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
            var serializedData = ConfigProvider.Serialize(configData);
            var encryptedContent = await EncryptionProvider.EncryptAsync(serializedData, Password);
            await ConfigProvider.SaveAsStringAsync(filePath, encryptedContent);
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not EncryptionException && ex is not ArgumentException && ex is not ArgumentNullException)
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving configuration", ex);
        }
    }

    /// <summary>
    /// Loads configuration of the specified type from the given file path synchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration to load</typeparam>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The loaded configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be loaded</exception>
    /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    public T Load<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        try
        {
            var encryptedContent = ConfigProvider.LoadAsString(filePath);
            var decryptedContent = EncryptionProvider.Decrypt(encryptedContent, Password);
            return ConfigProvider.Deserialize<T>(decryptedContent);
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not EncryptionException && ex is not ArgumentException)
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
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be saved</exception>
    /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if configData is null.</exception>
    public void Save<T>(string filePath, T configData)
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
            var serializedData = ConfigProvider.Serialize(configData);
            var encryptedContent = EncryptionProvider.Encrypt(serializedData, Password);
            ConfigProvider.SaveAsString(filePath, encryptedContent);
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not EncryptionException && ex is not ArgumentException && ex is not ArgumentNullException)
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving configuration", ex);
        }
    }
}