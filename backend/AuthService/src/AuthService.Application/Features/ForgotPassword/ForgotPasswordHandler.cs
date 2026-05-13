using AuthService.Application.Database;
using AuthService.Domain;
using Core.Abstractions;
using Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Result;

namespace AuthService.Application.Features.ForgotPassword;

public sealed class ForgotPasswordHandler : IResultCommandHandler<ForgotPasswordCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<ForgotPasswordCommand> _validator;
    private readonly IEmailSender _emailSender;
    private readonly ITransactionManager _transactionManager;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<ForgotPasswordCommand> validator,
        IEmailSender emailSender,
        ITransactionManager transactionManager,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ForgotPasswordHandler> logger)
    {
        _userManager = userManager;
        _validator = validator;
        _emailSender = emailSender;
        _transactionManager = transactionManager;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var request = command.Request;
        string email = request.Email;

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogError("User {Email} not found", email);
            await _transactionManager.RollbackAsync(cancellationToken);
            return Result.Success();
        }

        string resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var userId = user.Id;

        var resetPasswordLink = _linkGenerator.GetUriByName(
                _httpContextAccessor.HttpContext!,
                "VerifyResetPassword",
                new {
                    userId,
                    token = Base64UrlEncoder.Encode(resetPasswordToken)
                });
        var sendResetPasswordEmailResult = await _emailSender.SendPasswordResetAsync(
            email,
            resetPasswordLink!,
            cancellationToken);
        if (sendResetPasswordEmailResult.IsFailure)
        {
            return sendResetPasswordEmailResult.Errors;
        }

        return Result.Success();
    }
}