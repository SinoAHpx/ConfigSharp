using System.Text.Json;
using ConfigSharp.Exceptions;
using ConfigSharp.Interfaces;

namespace ConfigSharp.Providers.Json;

/// <summary>
/// Configuration provider for JSON files.
/// </summary>
/// <typeparam name="T">The type of configuration object to work with</typeparam>
public class JsonConfigProvider<T> : IConfigProvider<T>
{
    private readonly JsonSerializerOptions _serializerOptions;

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
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Loads configuration from the specified JSON file path asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the JSON configuration file</param>
    /// <returns>The deserialized configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the file cannot be read or parsed</exception>
    public async Task<T> LoadAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new ConfigReadException(filePath, "File does not exist");
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ConfigReadException(filePath, "File is empty or contains only whitespace");
            }

            var result = JsonSerializer.Deserialize<T>(jsonContent, _serializerOptions);

            if (result == null)
            {
                throw new ConfigReadException(filePath, "Failed to deserialize JSON content - result was null");
            }

            return result;
        }
        catch (FileNotFoundException ex)
        {
            throw new ConfigReadException(filePath, "File not found", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigReadException(filePath, "Access denied", ex);
        }
        catch (JsonException ex)
        {
            throw new ConfigReadException(filePath, $"Invalid JSON format: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is ConfigReadException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred", ex);
        }
    }

    /// <summary>
    /// Saves configuration to the specified JSON file path asynchronously.
    /// </summary>
    /// <param name="filePath">The path where to save the JSON configuration file</param>
    /// <param name="configData">The configuration object to serialize and save</param>
    /// <exception cref="ConfigReadException">Thrown when the file cannot be written</exception>
    public async Task SaveAsync(string filePath, T configData)
    {
        try
        {
            if (configData == null)
            {
                throw new ArgumentNullException(nameof(configData), "Configuration data cannot be null");
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var jsonContent = JsonSerializer.Serialize(configData, _serializerOptions);
            await File.WriteAllTextAsync(filePath, jsonContent);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigReadException(filePath, "Access denied while writing file", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new ConfigReadException(filePath, "Directory not found", ex);
        }
        catch (JsonException ex)
        {
            throw new ConfigReadException(filePath, $"Error serializing to JSON: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is ConfigReadException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving", ex);
        }
    }

    /// <summary>
    /// Loads configuration from the specified JSON file path synchronously.
    /// </summary>
    /// <param name="filePath">The path to the JSON configuration file</param>
    /// <returns>The deserialized configuration object</returns>
    /// <exception cref="ConfigReadException">Thrown when the file cannot be read or parsed</exception>
    public T Load(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new ConfigReadException(filePath, "File does not exist");
            }

            var jsonContent = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ConfigReadException(filePath, "File is empty or contains only whitespace");
            }

            var result = JsonSerializer.Deserialize<T>(jsonContent, _serializerOptions);

            if (result == null)
            {
                throw new ConfigReadException(filePath, "Failed to deserialize JSON content - result was null");
            }

            return result;
        }
        catch (FileNotFoundException ex)
        {
            throw new ConfigReadException(filePath, "File not found", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigReadException(filePath, "Access denied", ex);
        }
        catch (JsonException ex)
        {
            throw new ConfigReadException(filePath, $"Invalid JSON format: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is ConfigReadException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred", ex);
        }
    }

    /// <summary>
    /// Saves configuration to the specified JSON file path synchronously.
    /// </summary>
    /// <param name="filePath">The path where to save the JSON configuration file</param>
    /// <param name="configData">The configuration object to serialize and save</param>
    /// <exception cref="ConfigReadException">Thrown when the file cannot be written</exception>
    public void Save(string filePath, T configData)
    {
        try
        {
            if (configData == null)
            {
                throw new ArgumentNullException(nameof(configData), "Configuration data cannot be null");
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var jsonContent = JsonSerializer.Serialize(configData, _serializerOptions);
            File.WriteAllText(filePath, jsonContent);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ConfigReadException(filePath, "Access denied while writing file", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            throw new ConfigReadException(filePath, "Directory not found", ex);
        }
        catch (JsonException ex)
        {
            throw new ConfigReadException(filePath, $"Error serializing to JSON: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is ConfigReadException))
        {
            throw new ConfigReadException(filePath, "Unexpected error occurred while saving", ex);
        }
    }
}