using System.Text.Json;
using ConfigSharp.Exceptions;
using ConfigSharp.Interfaces;

namespace ConfigSharp.Providers.Json;

/// <summary>
/// Configuration provider for JSON files.
/// </summary>
public class JsonConfigProvider : IConfigProvider
{
    private readonly JsonSerializerOptions _serializerOptions = null!;

    /// <summary>
    /// Initializes a new instance of the JsonConfigProvider class.
    /// </summary>
    /// <param name="serializerOptions">Optional JSON serializer options</param>
    public JsonConfigProvider(JsonSerializerOptions? serializerOptions = null)
    {
        _serializerOptions = serializerOptions ?? new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = false
        };
    }
    
    public JsonConfigProvider ()
    {
        
    }

    /// <summary>
    /// Loads configuration from the specified JSON file path asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the JSON configuration file</param>
    /// <returns>The deserialized configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the file cannot be read or parsed</exception>
    public async Task<T> LoadAsync<T>(string filePath)
    {
        var jsonContent = await LoadAsStringAsync(filePath);
        return Deserialize<T>(jsonContent);
    }

    /// <summary>
    /// Saves configuration to the specified JSON file path asynchronously.
    /// </summary>
    /// <param name="filePath">The path where to save the JSON configuration file</param>
    /// <param name="configData">The configuration object to serialize and save</param>
    /// <exception cref="ConfigReadException">Thrown when the file cannot be written</exception>
    public async Task SaveAsync<T>(string filePath, T configData)
    {
        var jsonContent = Serialize(configData);
        await SaveAsStringAsync(filePath, jsonContent);
    }

    /// <summary>
    /// Loads configuration from the specified JSON file path synchronously.
    /// </summary>
    /// <param name="filePath">The path to the JSON configuration file</param>
    /// <returns>The deserialized configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the file cannot be read or parsed</exception>
    public T Load<T>(string filePath)
    {
        var jsonContent = LoadAsString(filePath);
        return Deserialize<T>(jsonContent);
    }

    /// <summary>
    /// Saves configuration to the specified JSON file path synchronously.
    /// </summary>
    /// <param name="filePath">The path where to save the JSON configuration file</param>
    /// <param name="configData">The configuration object to serialize and save</param>
    /// <exception cref="ConfigReadException">Thrown when the file cannot be written</exception>
    public void Save<T>(string filePath, T configData)
    {
        var jsonContent = Serialize(configData);
        SaveAsString(filePath, jsonContent);
    }

    /// <summary>
    /// Loads configuration content from the specified file path as a string asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The content of the file as a string</returns>
    public async Task<string> LoadAsStringAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new ConfigReadException(filePath, "File does not exist");
            }

            var content = await File.ReadAllTextAsync(filePath);

            if (string.IsNullOrWhiteSpace(content))
            {
                // Consider if this should throw or return empty string, based on expected behavior.
                // For now, aligning with previous behavior which would lead to deserialize error.
                throw new ConfigReadException(filePath, "File is empty or contains only whitespace");
            }
            return content;
        }
        catch (FileNotFoundException ex)
        {
            throw new ConfigReadException(filePath, "File not found", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigReadException(filePath, "Access denied", ex);
        }
        catch (Exception ex) when (!(ex is ConfigReadException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while loading string content", ex);
        }
    }

    /// <summary>
    /// Saves configuration content to the specified file path as a string asynchronously.
    /// </summary>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="content">The string content to save</param>
    public async Task SaveAsStringAsync(string filePath, string content)
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await File.WriteAllTextAsync(filePath, content);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigReadException(filePath, "Access denied while writing file", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new ConfigReadException(filePath, "Directory not found", ex);
        }
        catch (Exception ex) when (!(ex is ConfigReadException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving string content", ex);
        }
    }

    /// <summary>
    /// Loads configuration content from the specified file path as a string synchronously.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The content of the file as a string</returns>
    public string LoadAsString(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new ConfigReadException(filePath, "File does not exist");
            }
            var content = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ConfigReadException(filePath, "File is empty or contains only whitespace");
            }
            return content;
        }
        catch (FileNotFoundException ex)
        {
            throw new ConfigReadException(filePath, "File not found", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigReadException(filePath, "Access denied", ex);
        }
        catch (Exception ex) when (!(ex is ConfigReadException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while loading string content", ex);
        }
    }

    /// <summary>
    /// Saves configuration content to the specified file path as a string synchronously.
    /// </summary>
    /// <param name="filePath">The path where to save the configuration file</param>
    /// <param name="content">The string content to save</param>
    public void SaveAsString(string filePath, string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(filePath, content);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigReadException(filePath, "Access denied while writing file", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new ConfigReadException(filePath, "Directory not found", ex);
        }
        catch (Exception ex) when (!(ex is ConfigReadException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving string content", ex);
        }
    }

    /// <summary>
    /// Serializes the given configuration object into a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the configuration object</typeparam>
    /// <param name="configData">The configuration object to serialize</param>
    /// <returns>A JSON string representation of the configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown if serialization fails</exception>
    public string Serialize<T>(T configData)
    {
        try
        {
            if (configData == null)
            {
                // Returning "null" for null configData to mimic JsonSerializer behavior for reference types.
                // For value types, JsonSerializer would produce default value (e.g. 0 for int).
                // Consider if specific handling for null is needed, e.g., throwing ArgumentNullException.
                return "null";
            }
            return JsonSerializer.Serialize(configData, _serializerOptions);
        }
        catch (JsonException ex)
        {
            throw new ConfigReadException(null, $"Error serializing to JSON: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is ConfigReadException))
        {
            throw new ConfigReadException(null, "Unexpected error occurred during serialization", ex);
        }
    }

    /// <summary>
    /// Deserializes the given JSON string content into a configuration object.
    /// </summary>
    /// <typeparam name="T">The type of the configuration object</typeparam>
    /// <param name="content">The JSON string content to deserialize</param>
    /// <returns>The deserialized configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown if deserialization fails</exception>
    public T Deserialize<T>(string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ConfigReadException(null, "Cannot deserialize null or empty content.");
            }

            var result = JsonSerializer.Deserialize<T>(content, _serializerOptions);

            if (result == null)
            {
                // This case might indicate that the JSON content was "null" or represented an empty object/array
                // that deserialized to null for a nullable reference type T.
                // Depending on strictness, could throw or return default(T) or allow null.
                // For now, throwing to be explicit about potential issues.
                throw new ConfigReadException(null, "Failed to deserialize JSON content - result was null");
            }
            return result;
        }
        catch (JsonException ex)
        {
            throw new ConfigReadException(null, $"Invalid JSON format: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is ConfigReadException))
        {
            throw new ConfigReadException(null, "Unexpected error occurred during deserialization", ex);
        }
    }
}