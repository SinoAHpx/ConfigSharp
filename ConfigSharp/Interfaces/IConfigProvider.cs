namespace ConfigSharp.Interfaces;

/// <summary>
/// Interface for configuration providers that can read and write configuration files.
/// </summary>
public interface IConfigProvider
{
    /// <summary>
    /// Loads configuration from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The deserialized configuration object</returns>
    Task<T> LoadAsync<T>(string filePath);

    /// <summary>
    /// Saves configuration to the specified file path.
    /// </summary>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="configData">The configuration object to serialize and save</param>
    Task SaveAsync<T>(string filePath, T configData);

    /// <summary>
    /// Loads configuration from the specified file path synchronously.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The deserialized configuration object</returns>
    T Load<T>(string filePath);

    /// <summary>
    /// Saves configuration to the specified file path synchronously.
    /// </summary>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="configData">The configuration object to serialize and save</param>
    void Save<T>(string filePath, T configData);

    /// <summary>
    /// Loads configuration content from the specified file path as a string asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The content of the file as a string</returns>
    Task<string> LoadAsStringAsync(string filePath);

    /// <summary>
    /// Saves configuration content to the specified file path as a string asynchronously.
    /// </summary>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="content">The string content to save</param>
    Task SaveAsStringAsync(string filePath, string content);

    /// <summary>
    /// Loads configuration content from the specified file path as a string synchronously.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The content of the file as a string</returns>
    string LoadAsString(string filePath);

    /// <summary>
    /// Saves configuration content to the specified file path as a string synchronously.
    /// </summary>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="content">The string content to save</param>
    void SaveAsString(string filePath, string content);

    /// <summary>
    /// Serializes the given configuration object into a string.
    /// </summary>
    /// <typeparam name="T">The type of the configuration object</typeparam>
    /// <param name="configData">The configuration object to serialize</param>
    /// <returns>A string representation of the configuration object</returns>
    string Serialize<T>(T configData);

    /// <summary>
    /// Deserializes the given string content into a configuration object.
    /// </summary>
    /// <typeparam name="T">The type of the configuration object</typeparam>
    /// <param name="content">The string content to deserialize</param>
    /// <returns>The deserialized configuration object</returns>
    T Deserialize<T>(string content);
}