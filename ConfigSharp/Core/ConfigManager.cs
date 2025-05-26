using System.Text.Json;
using ConfigSharp.Encryption;
using ConfigSharp.Exceptions;
using ConfigSharp.Interfaces;
using ConfigSharp.Providers.Json;

namespace ConfigSharp.Core;

/// <summary>
/// Main configuration manager that orchestrates loading and saving configurations with optional encryption.
/// </summary>
public class ConfigManager : IConfigManager
{
    private readonly IEncryptionProvider _encryptionProvider;
    private readonly JsonSerializerOptions? _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the ConfigManager class with default providers.
    /// </summary>
    /// <param name="encryptionProvider">Optional encryption provider (defaults to AES)</param>
    /// <param name="jsonOptions">Optional JSON serializer options</param>
    public ConfigManager(IEncryptionProvider? encryptionProvider = null, JsonSerializerOptions? jsonOptions = null)
    {
        _encryptionProvider = encryptionProvider ?? new AesEncryptionProvider();
        _jsonOptions = jsonOptions;
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
    public async Task<T> LoadConfigAsync<T>(string filePath, string? password = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

            return fileExtension switch
            {
                ".json" => await LoadJsonConfigAsync<T>(filePath, password),
                _ => throw new ConfigReadException(filePath, $"Unsupported file format: {fileExtension}")
            };
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
    public async Task SaveConfigAsync<T>(string filePath, T configData, string? password = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (configData == null)
            {
                throw new ArgumentNullException(nameof(configData), "Configuration data cannot be null");
            }

            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

            switch (fileExtension)
            {
                case ".json":
                    await SaveJsonConfigAsync(filePath, configData, password);
                    break;
                default:
                    throw new ConfigReadException(filePath, $"Unsupported file format: {fileExtension}");
            }
        }
        catch (Exception ex) when (!(ex is ConfigReadException) && !(ex is EncryptionException) && !(ex is ArgumentException))
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
    public T LoadConfig<T>(string filePath, string? password = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

            return fileExtension switch
            {
                ".json" => LoadJsonConfig<T>(filePath, password),
                _ => throw new ConfigReadException(filePath, $"Unsupported file format: {fileExtension}")
            };
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
    public void SaveConfig<T>(string filePath, T configData, string? password = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (configData == null)
            {
                throw new ArgumentNullException(nameof(configData), "Configuration data cannot be null");
            }

            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

            switch (fileExtension)
            {
                case ".json":
                    SaveJsonConfig(filePath, configData, password);
                    break;
                default:
                    throw new ConfigReadException(filePath, $"Unsupported file format: {fileExtension}");
            }
        }
        catch (Exception ex) when (!(ex is ConfigReadException) && !(ex is EncryptionException) && !(ex is ArgumentException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving configuration", ex);
        }
    }

    #region JSON-specific methods

    private async Task<T> LoadJsonConfigAsync<T>(string filePath, string? password)
    {
        var jsonProvider = new JsonConfigProvider<T>(_jsonOptions);

        if (!string.IsNullOrEmpty(password))
        {
            // Load encrypted JSON
            var encryptedJsonProvider = new JsonConfigProvider<string>(_jsonOptions);
            var encryptedContent = await encryptedJsonProvider.LoadAsync(filePath);
            var decryptedContent = await _encryptionProvider.DecryptAsync(encryptedContent, password);

            // Parse the decrypted JSON
            var tempFilePath = Path.GetTempFileName();
            try
            {
                await File.WriteAllTextAsync(tempFilePath, decryptedContent);
                return await jsonProvider.LoadAsync(tempFilePath);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
        else
        {
            // Load unencrypted JSON
            return await jsonProvider.LoadAsync(filePath);
        }
    }

    private async Task SaveJsonConfigAsync<T>(string filePath, T configData, string? password)
    {
        var jsonProvider = new JsonConfigProvider<T>(_jsonOptions);

        if (!string.IsNullOrEmpty(password))
        {
            // Save encrypted JSON
            var tempFilePath = Path.GetTempFileName();
            try
            {
                await jsonProvider.SaveAsync(tempFilePath, configData);
                var jsonContent = await File.ReadAllTextAsync(tempFilePath);
                var encryptedContent = await _encryptionProvider.EncryptAsync(jsonContent, password);

                var encryptedJsonProvider = new JsonConfigProvider<string>(_jsonOptions);
                await encryptedJsonProvider.SaveAsync(filePath, encryptedContent);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
        else
        {
            // Save unencrypted JSON
            await jsonProvider.SaveAsync(filePath, configData);
        }
    }

    private T LoadJsonConfig<T>(string filePath, string? password)
    {
        var jsonProvider = new JsonConfigProvider<T>(_jsonOptions);

        if (!string.IsNullOrEmpty(password))
        {
            // Load encrypted JSON
            var encryptedJsonProvider = new JsonConfigProvider<string>(_jsonOptions);
            var encryptedContent = encryptedJsonProvider.Load(filePath);
            var decryptedContent = _encryptionProvider.Decrypt(encryptedContent, password);

            // Parse the decrypted JSON
            var tempFilePath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFilePath, decryptedContent);
                return jsonProvider.Load(tempFilePath);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
        else
        {
            // Load unencrypted JSON
            return jsonProvider.Load(filePath);
        }
    }

    private void SaveJsonConfig<T>(string filePath, T configData, string? password)
    {
        var jsonProvider = new JsonConfigProvider<T>(_jsonOptions);

        if (!string.IsNullOrEmpty(password))
        {
            // Save encrypted JSON
            var tempFilePath = Path.GetTempFileName();
            try
            {
                jsonProvider.Save(tempFilePath, configData);
                var jsonContent = File.ReadAllText(tempFilePath);
                var encryptedContent = _encryptionProvider.Encrypt(jsonContent, password);

                var encryptedJsonProvider = new JsonConfigProvider<string>(_jsonOptions);
                encryptedJsonProvider.Save(filePath, encryptedContent);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
        else
        {
            // Save unencrypted JSON
            jsonProvider.Save(filePath, configData);
        }
    }

    #endregion
}