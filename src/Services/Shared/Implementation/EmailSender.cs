namespace GustavoTech;

using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailSender : IEmailSender
{
    public EmailSender(
        IOptions<AuthMessageSenderOptions> optionsAccessor,
        ILogger<EmailSender> logger)
    {
        this.Options = optionsAccessor.Value;
        this.Logger = logger;
    }

    public ILogger<EmailSender> Logger { get; set; }

    public AuthMessageSenderOptions Options { get; }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        this.Logger.LogInformation("Sending email to {} with subject {}", email, subject);
        var responseCode = await this.Execute(this.Options.SendGridKey, subject, message, email);
        this.Logger.LogInformation("Email {HttpStatusCode}", responseCode);
    }

    public async Task<HttpStatusCode> Execute(string apiKey, string subject, string message, string email)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress($"noreply@{this.Options.FromEmail}"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message,
        };

        msg.AddTo(email);

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.TrackingSettings = new TrackingSettings
        {
            ClickTracking = new ClickTracking { Enable = false },
        };

        var response = await client.SendEmailAsync(msg);
        return response.StatusCode;
    }
}