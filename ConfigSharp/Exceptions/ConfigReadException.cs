namespace ConfigSharp.Exceptions;

/// <summary>
/// Exception thrown when there's an error reading a configuration file.
/// </summary>
public class ConfigReadException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ConfigReadException class.
    /// </summary>
    public ConfigReadException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConfigReadException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ConfigReadException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConfigReadException class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ConfigReadException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConfigReadException class with file path information.
    /// </summary>
    /// <param name="filePath">The path of the file that couldn't be read</param>
    /// <param name="message">The message that describes the error</param>
    public ConfigReadException(string? filePath, string message) : base($"Error reading config file '{filePath}': {message}")
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the ConfigReadException class with file path information and inner exception.
    /// </summary>
    /// <param name="filePath">The path of the file that couldn't be read</param>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ConfigReadException(string? filePath, string message, Exception innerException) : base($"Error reading config file '{filePath}': {message}", innerException)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Gets the file path associated with this exception.
    /// </summary>
    public string? FilePath { get; }
}