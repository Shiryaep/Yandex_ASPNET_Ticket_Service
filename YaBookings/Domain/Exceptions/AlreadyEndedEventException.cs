namespace YaBookings.Domain.Exceptions;

public class AlreadyEndedEventException : Exception
{
    public AlreadyEndedEventException() : base("Event has already ended")
    {
    }

    public AlreadyEndedEventException(string message) : base(message)
    {
    }

    public AlreadyEndedEventException(string message, Exception innerException) : base(message, innerException)
    {
    }
}