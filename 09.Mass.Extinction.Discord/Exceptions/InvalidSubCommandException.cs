namespace Ninth.Mass.Extinction.Discord.Exceptions;

[Serializable]
public class InvalidSubCommandException : Exception
{
    public InvalidSubCommandException() { }
    public InvalidSubCommandException(string message) : base(message) { }
    public InvalidSubCommandException(string message, Exception innerException) : base(message, innerException) { }
}
