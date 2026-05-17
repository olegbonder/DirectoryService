using AuthService.Application.Database;
using AuthService.Domain;
using AuthService.Domain.Permissions;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Result;

namespace AuthService.Application.Features.ReconfirmEmail;

public sealed class ReconfirmEmailHandler : IResultCommandHandler<ReconfirmEmailCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<ReconfirmEmailCommand> _validator;
    private readonly IEmailSender _emailSender;
    private readonly ITransactionManager _transactionManager;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ReconfirmEmailHandler> _logger;

    public ReconfirmEmailHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<ReconfirmEmailCommand> validator,
        IEmailSender emailSender,
        ITransactionManager transactionManager,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ReconfirmEmailHandler> logger)
    {
        _userManager = userManager;
        _validator = validator;
        _emailSender = emailSender;
        _transactionManager = transactionManager;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result> Handle(ReconfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        string email = command.Request.Email;
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogError("User {Email} not found", email);
            return Result.Success();
        }

        var isConfirmedEmail = await _userManager.IsEmailConfirmedAsync(user);
        if (isConfirmedEmail)
        {
            _logger.LogInformation("Email {Email} is already confirmed", email);
            return Result.Success();
        }

        string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var userId = user.Id;

        var confirmationLink = _linkGenerator.GetUriByName(
                _httpContextAccessor.HttpContext!,
                "ConfirmEmail",
                new {
                    userId,
                    token = Base64UrlEncoder.Encode(confirmationToken)
                });
        var sendEmailConfirmResult = await _emailSender.SendEmailConfirmationAsync(
            email,
            confirmationLink!,
            cancellationToken);
        if (sendEmailConfirmResult.IsFailure)
        {
            return sendEmailConfirmResult.Errors;
        }

        _logger.LogInformation("Email reconfirmation link sent to {Email}", email);

        return Result.Success();
    }
}