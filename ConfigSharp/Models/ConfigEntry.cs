namespace ConfigSharp.Models;

/// <summary>
/// Represents a configuration entry with key-value pairs and metadata.
/// </summary>
public class ConfigEntry
{
    /// <summary>
    /// Gets or sets the key of the configuration entry.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the configuration entry.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the description of the configuration entry.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether this configuration entry is required.
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets whether this configuration entry is encrypted.
    /// </summary>
    public bool IsEncrypted { get; set; } = false;

    /// <summary>
    /// Gets or sets the data type of the value.
    /// </summary>
    public string? DataType { get; set; }

    /// <summary>
    /// Gets or sets the default value for this configuration entry.
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the ConfigEntry class.
    /// </summary>
    public ConfigEntry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConfigEntry class with key and value.
    /// </summary>
    /// <param name="key">The key of the configuration entry</param>
    /// <param name="value">The value of the configuration entry</param>
    public ConfigEntry(string key, object? value)
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the ConfigEntry class with key, value, and description.
    /// </summary>
    /// <param name="key">The key of the configuration entry</param>
    /// <param name="value">The value of the configuration entry</param>
    /// <param name="description">The description of the configuration entry</param>
    public ConfigEntry(string key, object? value, string? description)
    {
        Key = key;
        Value = value;
        Description = description;
    }

    /// <summary>
    /// Gets the value as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to</typeparam>
    /// <returns>The value converted to the specified type</returns>
    public T? GetValue<T>()
    {
        if (Value == null)
            return default(T);

        if (Value is T directValue)
            return directValue;

        try
        {
            return (T)Convert.ChangeType(Value, typeof(T));
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    /// <summary>
    /// Returns a string representation of this configuration entry.
    /// </summary>
    /// <returns>A string that represents this configuration entry</returns>
    public override string ToString()
    {
        return $"{Key} = {Value ?? "null"}";
    }
}