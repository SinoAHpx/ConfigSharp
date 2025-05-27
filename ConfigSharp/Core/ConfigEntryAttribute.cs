using System;

namespace ConfigSharp.Core;

/// <summary>
/// Attribute used to mark properties in configuration classes with metadata.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ConfigEntryAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the configuration entry in the config file.
    /// If not specified, the property name is used.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets whether the configuration entry is required.
    /// If true and the value is null, an exception will be thrown.
    /// Default is true.
    /// </summary>
    public bool Required { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the configuration entry should be encrypted.
    /// Only applicable when using EncryptedConfigManager.
    /// Default is false.
    /// </summary>
    public bool Encrypt { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the ConfigEntryAttribute class.
    /// </summary>
    public ConfigEntryAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConfigEntryAttribute class with the specified name.
    /// </summary>
    /// <param name="name">The name of the configuration entry in the config file.</param>
    public ConfigEntryAttribute(string name)
    {
        Name = name;
    }
}