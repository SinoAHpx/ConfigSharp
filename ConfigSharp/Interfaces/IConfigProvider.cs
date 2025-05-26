namespace ConfigSharp.Interfaces;

/// <summary>
/// Interface for configuration providers that can read and write configuration files.
/// </summary>
/// <typeparam name="T">The type of configuration object to work with</typeparam>
public interface IConfigProvider<T>
{
    /// <summary>
    /// Loads configuration from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The deserialized configuration object</returns>
    Task<T> LoadAsync(string filePath);

    /// <summary>
    /// Saves configuration to the specified file path.
    /// </summary>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="configData">The configuration object to serialize and save</param>
    Task SaveAsync(string filePath, T configData);

    /// <summary>
    /// Loads configuration from the specified file path synchronously.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The deserialized configuration object</returns>
    T Load(string filePath);

    /// <summary>
    /// Saves configuration to the specified file path synchronously.
    /// </summary>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="configData">The configuration object to serialize and save</param>
    void Save(string filePath, T configData);
}