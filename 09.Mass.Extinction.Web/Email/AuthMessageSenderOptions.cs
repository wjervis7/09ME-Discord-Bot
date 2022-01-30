namespace _09.Mass.Extinction.Web.Email;

using MailKit.Security;

public class AuthMessageSenderOptions
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public Sender Sender { get; set; }
    public SecureSocketOptions Security { get; set; }
}

public class Sender
{
    public string Name { get; set; }
    public string Email { get; set; }
}
