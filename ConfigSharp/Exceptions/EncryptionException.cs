namespace ConfigSharp.Exceptions;

/// <summary>
/// Exception thrown when there's an error during encryption or decryption operations.
/// </summary>
public class EncryptionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the EncryptionException class.
    /// </summary>
    public EncryptionException() : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the EncryptionException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public EncryptionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the EncryptionException class with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public EncryptionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the EncryptionException class with operation information.
    /// </summary>
    /// <param name="operation">The operation that failed (e.g., "encryption", "decryption")</param>
    /// <param name="message">The message that describes the error</param>
    public EncryptionException(string operation, string message) : base($"Error during {operation}: {message}")
    {
        Operation = operation;
    }

    /// <summary>
    /// Initializes a new instance of the EncryptionException class with operation information and inner exception.
    /// </summary>
    /// <param name="operation">The operation that failed (e.g., "encryption", "decryption")</param>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public EncryptionException(string operation, string message, Exception innerException) : base($"Error during {operation}: {message}", innerException)
    {
        Operation = operation;
    }

    /// <summary>
    /// Gets the operation that was being performed when the error occurred.
    /// </summary>
    public string? Operation { get; }
}