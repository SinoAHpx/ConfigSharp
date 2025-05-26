using ConfigSharp.Core;
using ConfigSharp.Exceptions;
using ConfigSharp.Test.Mocks;
using NUnit.Framework;

namespace ConfigSharp.Test.Core;

/// <summary>
/// Unit tests for the ConfigManager class.
/// </summary>
[TestFixture]
public class ConfigManagerTests
{
    private ConfigManager _configManager;
    private MockEncryptionProvider _mockEncryption;
    private string _testDirectory;

    /// <summary>
    /// Test configuration model for testing purposes.
    /// </summary>
    public class TestConfig
    {
        public string DatabaseConnection { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public TestSettings Settings { get; set; } = new();
        public string[] Features { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Test settings sub-model.
    /// </summary>
    public class TestSettings
    {
        public bool EnableLogging { get; set; }
        public int MaxRetries { get; set; }
        public int Timeout { get; set; }
    }

    [SetUp]
    public void Setup()
    {
        _mockEncryption = new MockEncryptionProvider();
        _configManager = new ConfigManager(_mockEncryption);
        _testDirectory = Path.Combine(Path.GetTempPath(), "ConfigSharpTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Test]
    public async Task LoadConfigAsync_WithValidJsonFile_ReturnsCorrectConfig()
    {
        // Arrange
        var testConfig = new TestConfig
        {
            DatabaseConnection = "Server=localhost;Database=Test;",
            ApiKey = "test-key",
            Settings = new TestSettings { EnableLogging = true, MaxRetries = 5, Timeout = 60000 },
            Features = new[] { "feature1", "feature2" }
        };

        var filePath = Path.Combine(_testDirectory, "test.json");
        await _configManager.SaveConfigAsync(filePath, testConfig);

        // Act
        var loadedConfig = await _configManager.LoadConfigAsync<TestConfig>(filePath);

        // Assert
        Assert.That(loadedConfig, Is.Not.Null);
        Assert.That(loadedConfig.DatabaseConnection, Is.EqualTo(testConfig.DatabaseConnection));
        Assert.That(loadedConfig.ApiKey, Is.EqualTo(testConfig.ApiKey));
        Assert.That(loadedConfig.Settings.EnableLogging, Is.EqualTo(testConfig.Settings.EnableLogging));
        Assert.That(loadedConfig.Settings.MaxRetries, Is.EqualTo(testConfig.Settings.MaxRetries));
        Assert.That(loadedConfig.Features, Is.EqualTo(testConfig.Features));
    }

    [Test]
    public async Task LoadConfigAsync_WithEncryptedFile_ReturnsCorrectConfig()
    {
        // Arrange
        var testConfig = new TestConfig
        {
            DatabaseConnection = "Server=localhost;Database=Test;",
            ApiKey = "secret-key"
        };

        var filePath = Path.Combine(_testDirectory, "encrypted-test.json");
        var password = "test-password";

        await _configManager.SaveConfigAsync(filePath, testConfig, password);

        // Act
        var loadedConfig = await _configManager.LoadConfigAsync<TestConfig>(filePath, password);

        // Assert
        Assert.That(loadedConfig, Is.Not.Null);
        Assert.That(loadedConfig.DatabaseConnection, Is.EqualTo(testConfig.DatabaseConnection));
        Assert.That(loadedConfig.ApiKey, Is.EqualTo(testConfig.ApiKey));
    }

    [Test]
    public void LoadConfigAsync_WithNonExistentFile_ThrowsConfigReadException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.json");

        // Act & Assert
        Assert.ThrowsAsync<ConfigReadException>(async () =>
            await _configManager.LoadConfigAsync<TestConfig>(filePath));
    }

    [Test]
    public void LoadConfigAsync_WithInvalidPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _configManager.LoadConfigAsync<TestConfig>(""));
    }

    [Test]
    public void SaveConfigAsync_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.json");

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _configManager.SaveConfigAsync<TestConfig>(filePath, null!));
    }

    [Test]
    public async Task SaveConfigAsync_WithValidConfig_CreatesFile()
    {
        // Arrange
        var testConfig = new TestConfig
        {
            DatabaseConnection = "Server=localhost;Database=Test;",
            ApiKey = "test-key"
        };

        var filePath = Path.Combine(_testDirectory, "save-test.json");

        // Act
        await _configManager.SaveConfigAsync(filePath, testConfig);

        // Assert
        Assert.That(File.Exists(filePath), Is.True);

        // Verify content by loading it back
        var loadedConfig = await _configManager.LoadConfigAsync<TestConfig>(filePath);
        Assert.That(loadedConfig.DatabaseConnection, Is.EqualTo(testConfig.DatabaseConnection));
        Assert.That(loadedConfig.ApiKey, Is.EqualTo(testConfig.ApiKey));
    }

    [Test]
    public async Task SaveConfigAsync_WithEncryption_CreatesEncryptedFile()
    {
        // Arrange
        var testConfig = new TestConfig
        {
            DatabaseConnection = "Server=localhost;Database=Test;",
            ApiKey = "secret-key"
        };

        var filePath = Path.Combine(_testDirectory, "encrypted-save-test.json");
        var password = "test-password";

        // Act
        await _configManager.SaveConfigAsync(filePath, testConfig, password);

        // Assert
        Assert.That(File.Exists(filePath), Is.True);

        // The file should contain encrypted content (not plain JSON)
        var fileContent = await File.ReadAllTextAsync(filePath);
        Assert.That(fileContent, Does.Not.Contain("localhost")); // Original content should not be visible

        // Verify we can decrypt and load it back
        var loadedConfig = await _configManager.LoadConfigAsync<TestConfig>(filePath, password);
        Assert.That(loadedConfig.DatabaseConnection, Is.EqualTo(testConfig.DatabaseConnection));
    }

    [Test]
    public void LoadConfigAsync_WithUnsupportedFileFormat_ThrowsConfigReadException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.xml");

        // Act & Assert
        var ex = Assert.ThrowsAsync<ConfigReadException>(async () =>
            await _configManager.LoadConfigAsync<TestConfig>(filePath));

        Assert.That(ex.Message, Does.Contain("Unsupported file format"));
    }

    [Test]
    public async Task LoadConfig_SynchronousVersion_WorksCorrectly()
    {
        // Arrange
        var testConfig = new TestConfig
        {
            DatabaseConnection = "Server=localhost;Database=Test;",
            ApiKey = "sync-test-key"
        };

        var filePath = Path.Combine(_testDirectory, "sync-test.json");
        _configManager.SaveConfig(filePath, testConfig);

        // Act
        var loadedConfig = _configManager.LoadConfig<TestConfig>(filePath);

        // Assert
        Assert.That(loadedConfig, Is.Not.Null);
        Assert.That(loadedConfig.DatabaseConnection, Is.EqualTo(testConfig.DatabaseConnection));
        Assert.That(loadedConfig.ApiKey, Is.EqualTo(testConfig.ApiKey));
    }
}