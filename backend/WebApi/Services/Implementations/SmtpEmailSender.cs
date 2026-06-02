using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace WebApi.Services.Implementations;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(string to, string subject, string body, IReadOnlyList<EmailAttachment>? attachments = null)
    {
        var host = _configuration["Smtp:Host"];
        var from = _configuration["Smtp:From"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException("SMTP non configurato: impostare Smtp:Host e Smtp:From.");
        }

        var port = int.TryParse(_configuration["Smtp:Port"], out var parsedPort) ? parsedPort : 587;
        var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var parsedSsl) || parsedSsl;
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];

        using var message = new MailMessage
        {
            From = new MailAddress(from),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(from);
        foreach (var recipient in SplitRecipients(to))
        {
            message.Bcc.Add(recipient);
        }

        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
                mailAttachment.ContentDisposition!.DispositionType = DispositionTypeNames.Attachment;
                message.Attachments.Add(mailAttachment);
            }
        }

        if (message.Bcc.Count == 0)
        {
            throw new InvalidOperationException("Nessun destinatario email valido.");
        }

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        await client.SendMailAsync(message);
    }

    private static IEnumerable<string> SplitRecipients(string recipients)
    {
        return recipients
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(recipient => !string.IsNullOrWhiteSpace(recipient));
    }
}
