using ConfigSharp.Exceptions;
using ConfigSharp.Interfaces;

namespace ConfigSharp.Core;

/// <summary>
/// Configuration manager for plain (non-encrypted) configurations.
/// </summary>
public class ConfigManager<TProvider> : IConfigManager
    where TProvider : IConfigProvider
{
    public TProvider ConfigProvider { get; set; } = Activator.CreateInstance<TProvider>();

    public ConfigManager(TProvider? configProvider = default)
    {
        if (configProvider != null)
        {
            ConfigProvider = configProvider;
        }
    }

    /// <summary>
    /// Loads configuration of the specified type from the given file path asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration to load</typeparam>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The loaded configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the configuration cannot be loaded</exception>
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    public async Task<T> LoadAsync<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        try
        {
            return await ConfigProvider.LoadAsync<T>(filePath);
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not ArgumentException)
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
            await ConfigProvider.SaveAsync(filePath, configData);
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not ArgumentException && ex is not ArgumentNullException)
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
    /// <exception cref="ArgumentException">Thrown if filePath is null or empty.</exception>
    public T Load<T>(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        try
        {
            return ConfigProvider.Load<T>(filePath);
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not ArgumentException)
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
            ConfigProvider.Save(filePath, configData);
        }
        catch (Exception ex) when (ex is not ConfigReadException && ex is not ArgumentException && ex is not ArgumentNullException)
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving configuration", ex);
        }
    }
}