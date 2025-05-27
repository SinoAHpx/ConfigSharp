# ConfigSharp

A lightweight configuration management library for .NET with support for multiple file formats and optional encryption.

## Features

- Multiple format support (currently JSON, extensible for XML/YAML)
- Optional AES-256 encryption
- Async and sync APIs
- Strongly typed configuration classes
- Extensible architecture

## Installation

Add project reference:

```xml
<ProjectReference Include="../ConfigSharp/ConfigSharp.csproj" />
```

## Usage

### Basic Configuration

```csharp
using ConfigSharp.Core;

public class AppConfig
{
    public string DatabaseConnection { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool EnableLogging { get; set; }
}

var configManager = new ConfigManager();

// Save
var config = new AppConfig 
{ 
    DatabaseConnection = "Server=localhost;Database=MyApp;",
    ApiKey = "your-api-key",
    EnableLogging = true
};
await configManager.SaveConfigAsync("config.json", config);

// Load
var loadedConfig = await configManager.LoadConfigAsync<AppConfig>("config.json");
```

### Encrypted Configuration

```csharp
var password = "your-secure-password";

// Save with encryption
await configManager.SaveConfigAsync("config.json", config, password);

// Load encrypted
var loadedConfig = await configManager.LoadConfigAsync<AppConfig>("config.json", password);
```

### Custom Encryption

```csharp
public class MyEncryption : IEncryptionProvider
{
    public string Encrypt(string plainText, string password) { /* implementation */ }
    public string Decrypt(string cipherText, string password) { /* implementation */ }
    // Implement async methods
}

var configManager = new ConfigManager(new MyEncryption());
```

## Architecture

```
ConfigSharp/
├── Interfaces/           # Core contracts
├── Providers/           # File format handlers
├── Encryption/          # Encryption implementations
├── Exceptions/          # Custom exceptions
└── Core/               # Main ConfigManager
```

Key components:
- **ConfigManager**: Main API for loading/saving
- **IConfigProvider**: File format interface
- **IEncryptionProvider**: Encryption interface
- **AesEncryptionProvider**: Built-in AES-256 encryption

## Security

The built-in encryption uses:
- AES-256 in CBC mode
- PBKDF2 key derivation with SHA-256
- 10,000 iterations
- Random salt and IV

## Testing

```bash
dotnet test
```

## Requirements

- .NET 8.0+

## License

MIT License 