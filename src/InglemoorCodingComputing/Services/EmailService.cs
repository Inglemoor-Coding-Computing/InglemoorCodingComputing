namespace InglemoorCodingComputing.Services;

using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;

public class EmailService : IEmailService
{
    private readonly string _smtpFrom;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;

    public EmailService(IConfiguration configuration)
    {
        _smtpPort = int.Parse(configuration["SMTP:Port"]);
        _smtpServer = configuration["SMTP:Server"];
        _smtpFrom = configuration["SMTP:From"];
        _smtpUsername = configuration["SMTP:Username"];
        _smtpPassword = configuration["SMTP:Password"];
    }

    public void Send(string to, string subject, string text)
    {
        using MimeMessage email = new();
        email.To.Add(MailboxAddress.Parse(to));
        email.From.Add(MailboxAddress.Parse(_smtpFrom));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Plain) { Text = text };

        using SmtpClient smtp = new();
        smtp.Connect(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
        smtp.Authenticate(_smtpUsername, _smtpPassword);
        smtp.Send(email);
        smtp.Disconnect(true);
    }
}
