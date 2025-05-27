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
    public TProvider ConfigProvider {get; set;} = Activator.CreateInstance<TProvider>();
    public TEncryption EncryptionProvider {get; set;} = Activator.CreateInstance<TEncryption>();
    
    public ConfigManager(TProvider? configProvider = default, TEncryption? encryptionProvider = default)
    {
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
    /// <param name="password">Optional password for encrypted configurations</param>
    /// <returns>The loaded configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be loaded</exception>
    /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    public async Task<T> LoadAsync<T>(string filePath, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        try
        {
            if (!string.IsNullOrEmpty(password))
            {
                var encryptedContent = await ConfigProvider.LoadAsStringAsync(filePath);
                var decryptedContent = await EncryptionProvider.DecryptAsync(encryptedContent, password);
                return ConfigProvider.Deserialize<T>(decryptedContent);
            }

            return await ConfigProvider.LoadAsync<T>(filePath);
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
    /// <param name="password">Optional password to encrypt the configuration</param>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be saved</exception>
    /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if configData is null.</exception>
    public async Task SaveAsync<T>(string filePath, T configData, string? password = null)
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
                var serializedData = ConfigProvider.Serialize(configData);
                var encryptedContent = await EncryptionProvider.EncryptAsync(serializedData, password);
                await ConfigProvider.SaveAsStringAsync(filePath, encryptedContent);
            }
            else
            {
                await ConfigProvider.SaveAsync(filePath, configData);
            }
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
    /// <param name="password">Optional password for encrypted configurations</param>
    /// <returns>The loaded configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be loaded</exception>
    /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    public T Load<T>(string filePath, string? password = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        try
        {
            if (!string.IsNullOrEmpty(password))
            {
                var encryptedContent = ConfigProvider.LoadAsString(filePath);
                var decryptedContent = EncryptionProvider.Decrypt(encryptedContent, password);
                return ConfigProvider.Deserialize<T>(decryptedContent);
            }
            else
            {
                return ConfigProvider.Load<T>(filePath);
            }
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
    /// <param name="password">Optional password to encrypt the configuration</param>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be saved</exception>
    /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if configData is null.</exception>
    public void Save<T>(string filePath, T configData, string? password = null)
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
                var serializedData = ConfigProvider.Serialize(configData);
                var encryptedContent = EncryptionProvider.Encrypt(serializedData, password);
                ConfigProvider.SaveAsString(filePath, encryptedContent);
            }
            else
            {
                ConfigProvider.Save(filePath, configData);
            }
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not EncryptionException && ex is not ArgumentException && ex is not ArgumentNullException)
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving configuration", ex);
        }
    }
}