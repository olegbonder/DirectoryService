using AuthService.Application;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SharedKernel.Result;

namespace AuthService.Infrastructure.EmailSender;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly MailOptions _options;

    public EmailSender(IOptions<MailOptions> options, ILogger<EmailSender> logger)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<Result> SendEmailConfirmationAsync(
        string email,
        string confirmationLink,
        CancellationToken ct)
    {
        string htmlBody = $"Please confirm your account by clicking this link: {confirmationLink}";
        string subject = "Confirm your email";
        var result = await SendEmailAsync(email, htmlBody, subject, ct);
        if (result.IsFailure)
            return result.Errors;

        _logger.LogInformation("sending email confirmation successfully to {Email}",  email);
        return Result.Success();
    }

    public async Task<Result> SendPasswordResetAsync(string email, string resetLink, CancellationToken ct)
    {
        string htmlBody = $"Please reset your password by clicking this link: {resetLink}";
        string subject = "Reset your password";
        var result = await SendEmailAsync(email, htmlBody, subject, ct);
        if (result.IsFailure)
            return result.Errors;

        _logger.LogInformation("sending email for reset password successfully send to {Email}",  email);
        return Result.Success();
    }

    private async Task<Result> SendEmailAsync(string email, string htmlBody, string subject, CancellationToken ct)
    {
        var mail = new MimeMessage();

        mail.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));

        bool tryParse = MailboxAddress.TryParse(email, out var mailAddress);
        if (!tryParse)
        {
            return Error.Validation("email.parse.error", "Parse email not success");
        }

        mail.To.Add(mailAddress!);

        var body = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        mail.Body = body.ToMessageBody();
        mail.Subject = subject;

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, cancellationToken: ct);
            await client.AuthenticateAsync(_options.UserName, _options.Password, ct);
            await client.SendAsync(mail, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            return Error.Failure("email.send", ex.InnerException?.Message ?? ex.Message);
        }

        return Result.Success();
    }
}