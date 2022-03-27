namespace _09.Mass.Extinction.Discord.Exceptions;

using System.Runtime.Serialization;

[Serializable]
public class InvalidSubCommandException : Exception
{
    public InvalidSubCommandException() { }
    public InvalidSubCommandException(string message) : base(message) { }
    public InvalidSubCommandException(string message, Exception innerException) : base(message, innerException) { }
    protected InvalidSubCommandException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
