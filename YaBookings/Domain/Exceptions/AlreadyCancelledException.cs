namespace Domain.Exceptions;

public class AlreadyCancelledException : Exception
{
    public AlreadyCancelledException() : base("Booking already been cancelled")
    {
    }

    public AlreadyCancelledException(string message) : base(message)
    {
    }

    public AlreadyCancelledException(string message, Exception innerException) : base(message, innerException)
    {
    }
}