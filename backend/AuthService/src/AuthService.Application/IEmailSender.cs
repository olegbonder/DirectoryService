using SharedKernel.Result;

namespace AuthService.Application;

public interface IEmailSender
{
    Task<Result> SendEmailConfirmationAsync(string email, string confirmationLink, CancellationToken ct);

    Task<Result> SendPasswordResetAsync(string email, string resetLink, CancellationToken ct);
}