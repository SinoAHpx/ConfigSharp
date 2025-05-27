using ConfigSharp.Core;
using System.Text.Json;
using ConfigSharp.Encryption;
using ConfigSharp.Providers.Json;
using Technetium.Debug;
using Technetium.Text;

namespace ConfigSharp.Example;

/// <summary>
/// Example configuration class with ConfigEntry attributes
/// </summary>
public class AppConfig
{
    [ConfigEntry]
    public string? DatabaseConnection { get; set; }

    [ConfigEntry(Encrypt = true)]
    public string? ApiKey { get; set; }

    [ConfigEntry]
    public LoggingSettings? Logging { get; set; }

    [ConfigEntry(Required = false)]
    public List<string>? EnabledFeatures { get; set; }
}

/// <summary>
/// Logging configuration
/// </summary>
public class LoggingSettings
{
    [ConfigEntry]
    public bool EnableDebug { get; set; }

    [ConfigEntry(Name = "log_level")]
    public string LogLevel { get; set; } = "Information";

    [ConfigEntry(Name = "max_file_size")]
    public int MaxFileSizeMB { get; set; } = 10;
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("ConfigSharp Example - Property-Level Encryption");
        Console.WriteLine("=============================================\n");

        // Create a configuration manager with encryption
        var configManager = new EncryptedConfigManager<JsonConfigProvider, AesEncryptionProvider>("mySecretPassword123");

        // Create sample configuration
        var config = new AppConfig
        {
            DatabaseConnection = "Server=localhost;Database=ExampleApp;Trusted_Connection=true;",
            ApiKey = "sk-1234567890abcdef", // This will be encrypted because of the ConfigEntry attribute
            Logging = new LoggingSettings
            {
                EnableDebug = true,
                LogLevel = "Debug",
                MaxFileSizeMB = 50
            },
            EnabledFeatures = new List<string> { "Feature1", "Feature2" }
        };

        try
        {
            var configFile = "property-encrypted-config.json";

            // Save the configuration - only the ApiKey will be encrypted
            Console.WriteLine("1. Saving configuration with property-level encryption...");
            await configManager.SaveAsync(configFile, config);
            Console.WriteLine($"   ✓ Configuration saved to {configFile}");

            // Display the raw JSON to show that ApiKey is encrypted
            Console.WriteLine("\n2. Raw JSON content (note the encrypted ApiKey):");
            var rawJson = File.ReadAllText(configFile);
            Console.WriteLine(rawJson);

            // Load the configuration - ApiKey will be automatically decrypted
            Console.WriteLine("\n3. Loading configuration with automatic property decryption...");
            var loadedConfig = await configManager.LoadAsync<AppConfig>(configFile);
            Console.WriteLine("   ✓ Configuration loaded successfully");

            // Display the loaded configuration
            Console.WriteLine("\n4. Loaded configuration values:");
            Console.WriteLine($"   Database: {loadedConfig.DatabaseConnection}");
            Console.WriteLine($"   API Key: {loadedConfig.ApiKey}"); // This was automatically decrypted
            Console.WriteLine($"   Debug Enabled: {loadedConfig.Logging?.EnableDebug}");
            Console.WriteLine($"   Log Level: {loadedConfig.Logging?.LogLevel}");
            Console.WriteLine($"   Max File Size: {loadedConfig.Logging?.MaxFileSizeMB} MB");

            if (loadedConfig.EnabledFeatures != null && loadedConfig.EnabledFeatures.Any())
            {
                Console.WriteLine($"   Enabled Features: {string.Join(", ", loadedConfig.EnabledFeatures)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
    }


}