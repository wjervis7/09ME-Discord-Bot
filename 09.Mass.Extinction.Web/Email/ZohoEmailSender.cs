namespace _09.Mass.Extinction.Web.Email;

using System;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

public class ZohoEmailSender : IEmailSender
{
    public ZohoEmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
    {
        Options = optionsAccessor.Value;
    }

    public AuthMessageSenderOptions Options { get; }

    public Task SendEmailAsync(string email, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(message));
        }

        return SendEmailAsyncInternal(email, subject, message);
    }

    private async Task SendEmailAsyncInternal(string email, string subject, string message)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(Encoding.UTF8, Options.Sender.Name, Options.Sender.Email));
        mimeMessage.To.Add(MailboxAddress.Parse(email));
        mimeMessage.Subject = subject;
        mimeMessage.Body = new TextPart(TextFormat.Html) {
            Text = message
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(Options.Host, Options.Port, Options.Security);
        await smtp.AuthenticateAsync(Options.Username, Options.Password);
        await smtp.SendAsync(mimeMessage);
        await smtp.DisconnectAsync(true);
    }
}
