using AuthService.Domain;
using AuthService.Domain.Permissions;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Result;

namespace AuthService.Application.Features.RegisterUser;

public sealed class RegisterUserHandler : ICommandHandler<Guid, RegisterUserCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<RegisterUserCommand> _validator;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<RegisterUserCommand> validator,
        IEmailSender emailSender,
        ILogger<RegisterUserHandler> logger)
    {
        _userManager = userManager;
        _validator = validator;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var validResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validResult.IsValid == false)
        {
            return validResult.ToList();
        }

        var request = command.Request;
        string email = request.Email;
        var userResult = ApplicationUser.Create(email, email, request.FirstName, request.LastName);
        if (userResult.IsFailure)
            return userResult.Errors;

        var user = userResult.Value;
        var createUserResult = await _userManager.CreateAsync(userResult.Value);
        if (createUserResult.Succeeded == false)
        {
            return createUserResult.Errors.ToErrors();
        }

        string role = PlatformGroups.PARTICIPANT;
        var createUserRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (createUserRoleResult.Succeeded == false)
        {
            return createUserRoleResult.Errors.ToErrors();
        }

        _logger.LogInformation("Create user email: {Email} with {Role}", email, role);

        string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var userId = user.Id;
        string confirmationLink = "http://localhost:5016/api/auth/confirm-email/" +
                                    $"?userId={userId}" +
                                    $"&token={Base64UrlEncoder.Encode(confirmationToken)}";
        var sendEmailConfirmResult = await _emailSender.SendEmailConfirmationAsync(
            request.Email,
            confirmationLink,
            cancellationToken);
        if (sendEmailConfirmResult.IsFailure)
        {
            return sendEmailConfirmResult.Errors;
        }

        return userId;
    }
}