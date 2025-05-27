namespace ConfigSharp.Exceptions;

/// <summary>
/// Exception thrown when there's a validation error in a configuration property.
/// </summary>
public class ConfigValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ConfigValidationException class.
    /// </summary>
    public ConfigValidationException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConfigValidationException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ConfigValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConfigValidationException class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ConfigValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ConfigValidationException class with property information.
    /// </summary>
    /// <param name="propertyName">The name of the property that failed validation</param>
    /// <param name="message">The message that describes the error</param>
    public ConfigValidationException(string? propertyName, string message) : base($"Validation error for property '{propertyName}': {message}")
    {
        PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the property name associated with this exception.
    /// </summary>
    public string? PropertyName { get; }
}