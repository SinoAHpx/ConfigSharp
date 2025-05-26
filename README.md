# ConfigSharp

A modern, extensible configuration management library for .NET that supports multiple file formats and encryption out of the box.

## Features

- 🗂️ **Multiple Format Support**: Currently supports JSON with extensible architecture for XML, YAML, and more
- 🔐 **Built-in Encryption**: AES-256 encryption with PBKDF2 key derivation for secure configuration storage
- ⚡ **Async/Sync Support**: Both asynchronous and synchronous APIs available
- 🎯 **Strongly Typed**: Generic support for your custom configuration classes
- 🧪 **Fully Tested**: Comprehensive unit test coverage
- 🏗️ **Extensible Architecture**: Interface-based design for easy customization
- 🛡️ **Error Handling**: Custom exceptions with detailed error information

## Quick Start

### Installation

Add the project reference to your application:

```xml
<ProjectReference Include="../ConfigSharp/ConfigSharp.csproj" />
```

### Basic Usage

```csharp
using ConfigSharp.Core;

// Define your configuration class
public class AppConfig
{
    public string DatabaseConnection { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public AppSettings Settings { get; set; } = new();
}

public class AppSettings
{
    public bool EnableLogging { get; set; }
    public int MaxRetries { get; set; }
    public int Timeout { get; set; }
}

// Create a configuration manager
var configManager = new ConfigManager();

// Save configuration
var config = new AppConfig
{
    DatabaseConnection = "Server=localhost;Database=MyApp;",
    ApiKey = "your-api-key",
    Settings = new AppSettings 
    { 
        EnableLogging = true, 
        MaxRetries = 3, 
        Timeout = 30000 
    }
};

await configManager.SaveConfigAsync("config.json", config);

// Load configuration
var loadedConfig = await configManager.LoadConfigAsync<AppConfig>("config.json");
```

### Encrypted Configuration

```csharp
var configManager = new ConfigManager();
var password = "your-secure-password";

// Save encrypted configuration
await configManager.SaveConfigAsync("secure-config.json", config, password);

// Load encrypted configuration
var loadedConfig = await configManager.LoadConfigAsync<AppConfig>("secure-config.json", password);
```

### Custom Encryption Provider

```csharp
using ConfigSharp.Interfaces;
using ConfigSharp.Core;

// Implement your custom encryption
public class MyCustomEncryption : IEncryptionProvider
{
    public string Encrypt(string plainText, string password) 
    { 
        // Your encryption logic here
        return encryptedText;
    }
    
    public string Decrypt(string cipherText, string password) 
    { 
        // Your decryption logic here
        return decryptedText;
    }
    
    // Implement async methods...
}

// Use with ConfigManager
var configManager = new ConfigManager(new MyCustomEncryption());
```

## Architecture

### Project Structure

```
ConfigSharp/
├── Interfaces/                # Abstract contracts
│   ├── IConfigProvider.cs     # Configuration file provider interface
│   ├── IEncryptionProvider.cs # Encryption provider interface
│   └── IConfigManager.cs      # Main manager interface
│
├── Providers/                 # File format implementations
│   └── Json/
│       └── JsonConfigProvider.cs
│
├── Encryption/                # Encryption implementations
│   └── AesEncryptionProvider.cs
│
├── Models/                    # Data models
│   └── ConfigEntry.cs
│
├── Exceptions/                # Custom exceptions
│   ├── ConfigReadException.cs
│   └── EncryptionException.cs
│
└── Core/                      # Main orchestration
    └── ConfigManager.cs
```

### Key Components

- **ConfigManager**: Main class users interact with for loading/saving configurations
- **IConfigProvider**: Interface for different file format providers (JSON, XML, etc.)
- **IEncryptionProvider**: Interface for different encryption algorithms
- **AesEncryptionProvider**: Built-in AES-256 encryption with PBKDF2
- **JsonConfigProvider**: JSON file format support using System.Text.Json

## Security Features

### AES Encryption

The built-in `AesEncryptionProvider` uses:
- **AES-256** encryption in CBC mode
- **PBKDF2** key derivation with SHA-256
- **10,000 iterations** for key derivation
- **Random salt and IV** for each encryption
- **PKCS7 padding**

### Best Practices

1. **Use strong passwords** for encrypted configurations
2. **Store passwords securely** (environment variables, key vaults, etc.)
3. **Rotate encryption passwords** regularly
4. **Use different passwords** for different environments

## Testing

The library includes comprehensive unit tests using NUnit:

```bash
dotnet test
```

Test coverage includes:
- ✅ Basic configuration loading/saving
- ✅ Encryption/decryption functionality
- ✅ Error handling scenarios
- ✅ File format validation
- ✅ Edge cases and invalid inputs

## Extending ConfigSharp

### Adding New File Formats

1. Implement `IConfigProvider<T>`:

```csharp
public class XmlConfigProvider<T> : IConfigProvider<T>
{
    public async Task<T> LoadAsync(string filePath) 
    { 
        // XML loading logic
    }
    
    public async Task SaveAsync(string filePath, T configData) 
    { 
        // XML saving logic
    }
    
    // Implement sync methods...
}
```

2. Update `ConfigManager` to support the new format.

### Adding New Encryption Algorithms

1. Implement `IEncryptionProvider`:

```csharp
public class ChaCha20EncryptionProvider : IEncryptionProvider
{
    // Implement ChaCha20 encryption
}
```

2. Use with `ConfigManager`:

```csharp
var configManager = new ConfigManager(new ChaCha20EncryptionProvider());
```

## Requirements

- .NET 8.0 or later
- System.Text.Json (included in .NET)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Roadmap

- [ ] XML configuration provider
- [ ] YAML configuration provider  
- [ ] Configuration validation
- [ ] Configuration schema generation
- [ ] Configuration file watching/hot-reload
- [ ] Configuration merging and inheritance
- [ ] Environment-specific configuration files
- [ ] Configuration value interpolation 