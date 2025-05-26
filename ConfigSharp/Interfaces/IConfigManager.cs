namespace ConfigSharp.Interfaces;

/// <summary>
/// Interface for the main configuration manager that orchestrates loading and saving configurations.
/// </summary>
public interface IConfigManager
{
    /// <summary>
    /// Loads configuration of the specified type from the given file path.
    /// </summary>
    /// <typeparam name="T">The type of configuration to load</typeparam>
    /// <param name="filePath">The path to the configuration file</param>
    /// <param name="password">Optional password for encrypted configurations</param>
    /// <returns>The loaded configuration object</returns>
    Task<T> LoadConfigAsync<T>(string filePath, string? password = null);

    /// <summary>
    /// Saves the specified configuration to the given file path.
    /// </summary>
    /// <typeparam name="T">The type of configuration to save</typeparam>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="configData">The configuration object to save</param>
    /// <param name="password">Optional password to encrypt the configuration</param>
    Task SaveConfigAsync<T>(string filePath, T configData, string? password = null);

    /// <summary>
    /// Loads configuration of the specified type from the given file path synchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration to load</typeparam>
    /// <param name="filePath">The path to the configuration file</param>
    /// <param name="password">Optional password for encrypted configurations</param>
    /// <returns>The loaded configuration object</returns>
    T LoadConfig<T>(string filePath, string? password = null);

    /// <summary>
    /// Saves the specified configuration to the given file path synchronously.
    /// </summary>
    /// <typeparam name="T">The type of configuration to save</typeparam>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="configData">The configuration object to save</param>
    /// <param name="password">Optional password to encrypt the configuration</param>
    void SaveConfig<T>(string filePath, T configData, string? password = null);
}