using ConfigSharp.Core;
using System.Text.Json;

namespace ConfigSharp.Example;

/// <summary>
/// Example configuration class
/// </summary>
public class AppConfig
{
    public string DatabaseConnection { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public LoggingSettings Logging { get; set; } = new();
    public List<string> EnabledFeatures { get; set; } = new();
}

/// <summary>
/// Logging configuration
/// </summary>
public class LoggingSettings
{
    public bool EnableDebug { get; set; }
    public string LogLevel { get; set; } = "Information";
    public int MaxFileSizeMB { get; set; } = 10;
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("ConfigSharp Example Demo");
        Console.WriteLine("========================\n");

        // Create a configuration manager
        var configManager = new ConfigManager();

        // Create sample configuration
        var config = new AppConfig
        {
            DatabaseConnection = "Server=localhost;Database=ExampleApp;Trusted_Connection=true;",
            ApiKey = "sk-1234567890abcdef",
            Logging = new LoggingSettings
            {
                EnableDebug = true,
                LogLevel = "Debug",
                MaxFileSizeMB = 50
            },
            EnabledFeatures = new List<string> { "UserAuth", "ApiRateLimit", "Analytics" }
        };

        try
        {
            // Demo 1: Save and load unencrypted configuration
            Console.WriteLine("1. Saving unencrypted configuration...");
            await configManager.SaveConfigAsync("example-config.json", config);
            Console.WriteLine("   ✓ Configuration saved to example-config.json");

            Console.WriteLine("\n2. Loading unencrypted configuration...");
            var loadedConfig = await configManager.LoadConfigAsync<AppConfig>("example-config.json");
            Console.WriteLine($"   ✓ Database: {loadedConfig.DatabaseConnection}");
            Console.WriteLine($"   ✓ API Key: {loadedConfig.ApiKey}");
            Console.WriteLine($"   ✓ Log Level: {loadedConfig.Logging.LogLevel}");
            Console.WriteLine($"   ✓ Features: {string.Join(", ", loadedConfig.EnabledFeatures)}");

            // Demo 2: Save and load encrypted configuration
            Console.WriteLine("\n3. Saving encrypted configuration...");
            string password = "MySecurePassword123!";
            await configManager.SaveConfigAsync("secure-config.json", config, password);
            Console.WriteLine("   ✓ Encrypted configuration saved to secure-config.json");

            Console.WriteLine("\n4. Loading encrypted configuration...");
            var decryptedConfig = await configManager.LoadConfigAsync<AppConfig>("secure-config.json", password);
            Console.WriteLine($"   ✓ Database: {decryptedConfig.DatabaseConnection}");
            Console.WriteLine($"   ✓ API Key: {decryptedConfig.ApiKey}");
            Console.WriteLine($"   ✓ Log Level: {decryptedConfig.Logging.LogLevel}");

            // Demo 3: Show the difference between encrypted and unencrypted files
            Console.WriteLine("\n5. File content comparison:");
            Console.WriteLine("\n   Unencrypted file content:");
            var unencryptedContent = await File.ReadAllTextAsync("example-config.json");
            Console.WriteLine($"   {unencryptedContent}");

            Console.WriteLine("\n   Encrypted file content (first 100 characters):");
            var encryptedContent = await File.ReadAllTextAsync("secure-config.json");
            var preview = encryptedContent.Length > 100 ? encryptedContent.Substring(0, 100) + "..." : encryptedContent;
            Console.WriteLine($"   {preview}");

            // Demo 4: Synchronous operations
            Console.WriteLine("\n6. Demonstrating synchronous operations...");
            configManager.SaveConfig("sync-config.json", config);
            var syncConfig = configManager.LoadConfig<AppConfig>("sync-config.json");
            Console.WriteLine($"   ✓ Synchronous load successful: {syncConfig.DatabaseConnection}");

            Console.WriteLine("\n✅ All demonstrations completed successfully!");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
        finally
        {
            // Clean up example files
            Console.WriteLine("\n7. Cleaning up example files...");
            DeleteFileIfExists("example-config.json");
            DeleteFileIfExists("secure-config.json");
            DeleteFileIfExists("sync-config.json");
            Console.WriteLine("   ✓ Cleanup completed");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static void DeleteFileIfExists(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"   ✓ Deleted {filePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ⚠ Could not delete {filePath}: {ex.Message}");
        }
    }
}