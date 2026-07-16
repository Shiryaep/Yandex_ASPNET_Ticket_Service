namespace YaEvents.Domain.Exceptions;

public class BookingLimitExceededException : Exception
{
    public BookingLimitExceededException() : base("Booking limit has been exceeded")
    {
    }

    public BookingLimitExceededException(string message) : base(message)
    {
    }

    public BookingLimitExceededException(string message, Exception innerException) : base(message, innerException)
    {
    }
}