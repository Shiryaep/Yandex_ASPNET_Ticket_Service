namespace YaBookings.Domain.Exceptions;

public class LackOfRightsException : Exception
{
    public LackOfRightsException() : base("Lack of rights to the operation")
    {
    }

    public LackOfRightsException(string message) : base(message)
    {
    }

    public LackOfRightsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}