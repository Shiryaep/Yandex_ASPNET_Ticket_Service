namespace YaUsers.Domain.Exceptions;

public class UserAlreadyAddedException : Exception
{
    public UserAlreadyAddedException() : base("User already been registered")
    {
    }

    public UserAlreadyAddedException(string message) : base(message)
    {
    }

    public UserAlreadyAddedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}