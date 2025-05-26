using ConfigSharp.Core;
using System.Text.Json;
using Technetium.Debug;
using Technetium.Text;

namespace ConfigSharp.Example;

/// <summary>
/// Example configuration class
/// </summary>
public class AppConfig
{
    public string? DatabaseConnection { get; set; }
    public string? ApiKey { get; set; }
    public LoggingSettings? Logging { get; set; }
    public List<string>? EnabledFeatures { get; set; }
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
        };

        try
        {
            await configManager.SaveConfigAsync("example2-config.json", config);
            var loadedConfig = await configManager.LoadConfigAsync<AppConfig>("example2-config.json");
            Console.WriteLine(loadedConfig.EnabledFeatures?.Count == null);
            Console.WriteLine(loadedConfig.ApiKey == null);

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
            // Console.WriteLine("\n7. Cleaning up example files...");
            // DeleteFileIfExists("example-config.json");
            // DeleteFileIfExists("secure-config.json");
            // DeleteFileIfExists("sync-config.json");
            // Console.WriteLine("   ✓ Cleanup completed");
        }

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