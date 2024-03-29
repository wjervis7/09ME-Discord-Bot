﻿namespace Ninth.Mass.Extinction.Web.Email;

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
