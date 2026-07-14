namespace Domain.Exceptions;

public class ConfigurationException : Exception
{
    public ConfigurationException() : base("Configuration component missing")
    {
    }

    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}