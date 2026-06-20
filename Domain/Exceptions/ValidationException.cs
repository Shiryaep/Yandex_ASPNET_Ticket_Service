namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when validation failed
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a default message
    /// </summary>
    public ValidationException() : base("Validation failed")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message
    /// </summary>
    /// <param name="message">The error message</param>
    public ValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}