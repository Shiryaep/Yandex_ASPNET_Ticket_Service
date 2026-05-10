using System;

namespace Yandex_ASPNET_Ticket_Service.Models;

/// <summary>
/// Exception thrown when there are no available seats for booking
/// </summary>
public class NoAvailableSeatsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoAvailableSeatsException"/> class with a default message
    /// </summary>
    public NoAvailableSeatsException() : base("No available seats for this event")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoAvailableSeatsException"/> class with a custom message
    /// </summary>
    /// <param name="message">The error message</param>
    public NoAvailableSeatsException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NoAvailableSeatsException"/> class with a custom message and inner exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public NoAvailableSeatsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}