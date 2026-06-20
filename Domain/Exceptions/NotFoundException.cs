using System;

namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when there are no available seats for booking
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a default message
    /// </summary>
    public NotFoundException() : base("The entity was not found")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message
    /// </summary>
    /// <param name="message">The error message</param>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a custom message and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}