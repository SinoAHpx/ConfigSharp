using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConfigSharp.Encryption;
using ConfigSharp.Exceptions;
using ConfigSharp.Interfaces;

namespace ConfigSharp.Core;

/// <summary>
/// Configuration manager that orchestrates loading and saving configurations with encryption support.
/// Supports property-level encryption using the ConfigEntry attribute.
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
    /// Handles property-level encryption based on ConfigEntry attributes.
    /// </summary>
    /// <typeparam name="T">The type of configuration to load</typeparam>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The loaded configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be loaded</exception>
    /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    /// <exception cref="ConfigValidationException">Thrown when a required property is null</exception>
    public async Task<T> LoadAsync<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        try
        {
            var content = await ConfigProvider.LoadAsStringAsync(filePath);

            // Parse the content as JSON to handle property-level encryption
            var jsonDocument = JsonDocument.Parse(content);
            var jsonObject = JsonSerializer.Deserialize<JsonNode>(content);

            if (jsonObject is JsonObject configObject)
            {
                // Process encrypted properties
                await ProcessEncryptedPropertiesForLoadingAsync<T>(configObject);

                // Serialize back to string for final deserialization
                content = jsonObject.ToJsonString();
            }

            // Deserialize the processed content
            var config = ConfigProvider.Deserialize<T>(content);

            // Validate required properties
            ValidateRequiredProperties(config);

            return config;
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not EncryptionException &&
                                 ex is not ArgumentException && ex is not ConfigValidationException)
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while loading configuration", ex);
        }
    }

    /// <summary>
    /// Saves the specified configuration to the given file path asynchronously.
    /// Handles property-level encryption based on ConfigEntry attributes.
    /// </summary>
    /// <typeparam name="T">The type of configuration to save</typeparam>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="configData">The configuration object to save</param>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be saved</exception>
    /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if configData is null.</exception>
    /// <exception cref="ConfigValidationException">Thrown when a required property is null</exception>
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
            // Validate required properties
            ValidateRequiredProperties(configData);

            // Serialize the data to JSON
            var serializedData = ConfigProvider.Serialize(configData);

            // Parse the serialized data to handle property-level encryption
            var jsonObject = JsonSerializer.Deserialize<JsonNode>(serializedData);

            if (jsonObject is JsonObject configObject)
            {
                // Process encrypted properties
                await ProcessEncryptedPropertiesForSavingAsync<T>(configObject);

                // Serialize back to string
                serializedData = jsonObject.ToJsonString();
            }

            // Save the processed content
            await ConfigProvider.SaveAsStringAsync(filePath, serializedData);
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not EncryptionException &&
                                 ex is not ArgumentException && ex is not ArgumentNullException &&
                                 ex is not ConfigValidationException)
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving configuration", ex);
        }
    }

    /// <summary>
    /// Loads configuration of the specified type from the given file path synchronously.
    /// Handles property-level encryption based on ConfigEntry attributes.
    /// </summary>
    /// <typeparam name="T">The type of configuration to load</typeparam>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The loaded configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be loaded</exception>
    /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    /// <exception cref="ConfigValidationException">Thrown when a required property is null</exception>
    public T Load<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        try
        {
            var content = ConfigProvider.LoadAsString(filePath);

            // Parse the content as JSON to handle property-level encryption
            var jsonDocument = JsonDocument.Parse(content);
            var jsonObject = JsonSerializer.Deserialize<JsonNode>(content);

            if (jsonObject is JsonObject configObject)
            {
                // Process encrypted properties
                ProcessEncryptedPropertiesForLoading<T>(configObject);

                // Serialize back to string for final deserialization
                content = jsonObject.ToJsonString();
            }

            // Deserialize the processed content
            var config = ConfigProvider.Deserialize<T>(content);

            // Validate required properties
            ValidateRequiredProperties(config);

            return config;
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not EncryptionException &&
                                 ex is not ArgumentException && ex is not ConfigValidationException)
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while loading configuration", ex);
        }
    }

    /// <summary>
    /// Saves the specified configuration to the given file path synchronously.
    /// Handles property-level encryption based on ConfigEntry attributes.
    /// </summary>
    /// <typeparam name="T">The type of configuration to save</typeparam>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="configData">The configuration object to save</param>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be saved</exception>
    /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if configData is null.</exception>
    /// <exception cref="ConfigValidationException">Thrown when a required property is null</exception>
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
            // Validate required properties
            ValidateRequiredProperties(configData);

            // Serialize the data to JSON
            var serializedData = ConfigProvider.Serialize(configData);

            // Parse the serialized data to handle property-level encryption
            var jsonObject = JsonSerializer.Deserialize<JsonNode>(serializedData);

            if (jsonObject is JsonObject configObject)
            {
                // Process encrypted properties
                ProcessEncryptedPropertiesForSaving<T>(configObject);

                // Serialize back to string
                serializedData = jsonObject.ToJsonString();
            }

            // Save the processed content
            ConfigProvider.SaveAsString(filePath, serializedData);
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not EncryptionException &&
                                 ex is not ArgumentException && ex is not ArgumentNullException &&
                                 ex is not ConfigValidationException)
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving configuration", ex);
        }
    }

    /// <summary>
    /// Validates that all required properties have non-null values.
    /// </summary>
    /// <typeparam name="T">The type of configuration to validate</typeparam>
    /// <param name="configData">The configuration object to validate</param>
    /// <exception cref="ConfigValidationException">Thrown when a required property is null</exception>
    private void ValidateRequiredProperties<T>(T configData)
    {
        if (configData == null)
        {
            return;
        }

        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            var configEntryAttribute = property.GetCustomAttribute<ConfigEntryAttribute>();
            if (configEntryAttribute != null && configEntryAttribute.Required)
            {
                var value = property.GetValue(configData);
                if (value == null)
                {
                    throw new ConfigValidationException(property.Name, "Required property cannot be null");
                }
            }
        }
    }

    /// <summary>
    /// Processes encrypted properties for saving.
    /// </summary>
    /// <typeparam name="T">The type of configuration being saved</typeparam>
    /// <param name="jsonObject">The JSON object representing the configuration</param>
    private void ProcessEncryptedPropertiesForSaving<T>(JsonObject jsonObject)
    {
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            var configEntryAttribute = property.GetCustomAttribute<ConfigEntryAttribute>();
            if (configEntryAttribute != null && configEntryAttribute.Encrypt)
            {
                var propertyName = configEntryAttribute.Name ?? property.Name;
                if (jsonObject.TryGetPropertyValue(propertyName, out var propertyValue) && propertyValue != null)
                {
                    var stringValue = propertyValue.ToString();
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        var encryptedValue = EncryptionProvider.Encrypt(stringValue, Password);
                        jsonObject[propertyName] = encryptedValue;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Processes encrypted properties for saving asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration being saved</typeparam>
    /// <param name="jsonObject">The JSON object representing the configuration</param>
    private async Task ProcessEncryptedPropertiesForSavingAsync<T>(JsonObject jsonObject)
    {
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            var configEntryAttribute = property.GetCustomAttribute<ConfigEntryAttribute>();
            if (configEntryAttribute != null && configEntryAttribute.Encrypt)
            {
                var propertyName = configEntryAttribute.Name ?? property.Name;
                if (jsonObject.TryGetPropertyValue(propertyName, out var propertyValue) && propertyValue != null)
                {
                    var stringValue = propertyValue.ToString();
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        var encryptedValue = await EncryptionProvider.EncryptAsync(stringValue, Password);
                        jsonObject[propertyName] = encryptedValue;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Processes encrypted properties for loading.
    /// </summary>
    /// <typeparam name="T">The type of configuration being loaded</typeparam>
    /// <param name="jsonObject">The JSON object representing the configuration</param>
    private void ProcessEncryptedPropertiesForLoading<T>(JsonObject jsonObject)
    {
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            var configEntryAttribute = property.GetCustomAttribute<ConfigEntryAttribute>();
            if (configEntryAttribute != null && configEntryAttribute.Encrypt)
            {
                var propertyName = configEntryAttribute.Name ?? property.Name;
                if (jsonObject.TryGetPropertyValue(propertyName, out var propertyValue) && propertyValue != null)
                {
                    var encryptedValue = propertyValue.ToString();
                    if (!string.IsNullOrEmpty(encryptedValue))
                    {
                        var decryptedValue = EncryptionProvider.Decrypt(encryptedValue, Password);
                        jsonObject[propertyName] = decryptedValue;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Processes encrypted properties for loading asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration being loaded</typeparam>
    /// <param name="jsonObject">The JSON object representing the configuration</param>
    private async Task ProcessEncryptedPropertiesForLoadingAsync<T>(JsonObject jsonObject)
    {
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            var configEntryAttribute = property.GetCustomAttribute<ConfigEntryAttribute>();
            if (configEntryAttribute != null && configEntryAttribute.Encrypt)
            {
                var propertyName = configEntryAttribute.Name ?? property.Name;
                if (jsonObject.TryGetPropertyValue(propertyName, out var propertyValue) && propertyValue != null)
                {
                    var encryptedValue = propertyValue.ToString();
                    if (!string.IsNullOrEmpty(encryptedValue))
                    {
                        var decryptedValue = await EncryptionProvider.DecryptAsync(encryptedValue, Password);
                        jsonObject[propertyName] = decryptedValue;
                    }
                }
            }
        }
    }
}