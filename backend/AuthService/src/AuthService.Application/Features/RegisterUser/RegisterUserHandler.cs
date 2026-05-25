using AuthService.Application.Database;
using AuthService.Domain;
using AuthService.Domain.Shared;
using Core.Abstractions;
using Core.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SharedAuth.Constants;
using SharedKernel.Result;

namespace AuthService.Application.Features.RegisterUser;

public sealed class RegisterUserHandler : ICommandHandler<Guid, RegisterUserCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<RegisterUserCommand> _validator;
    private readonly IEmailSender _emailSender;
    private readonly ITransactionManager _transactionManager;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<RegisterUserCommand> validator,
        IEmailSender emailSender,
        ITransactionManager transactionManager,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RegisterUserHandler> logger)
    {
        _userManager = userManager;
        _validator = validator;
        _emailSender = emailSender;
        _transactionManager = transactionManager;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
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

        await _transactionManager.BeginTransactionAsync(cancellationToken);

        var user = userResult.Value;
        var createUserResult = await _userManager.CreateAsync(userResult.Value, request.Password);
        if (createUserResult.Succeeded == false)
        {
            _logger.LogError(
                "Failed to create user {Email} exception: {Exception}",
                email,
                createUserResult.Errors.ToErrorString());
            await _transactionManager.RollbackAsync(cancellationToken);
            return createUserResult.Errors.ToErrors();
        }

        string role = PlatformGroups.PARTICIPANT;
        var createUserRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (createUserRoleResult.Succeeded == false)
        {
            _logger.LogError(
                "Failed to add role {Role} to user {Email} exception: {Exception}",
                role,
                email,
                createUserResult.Errors.ToErrorString());
            await _transactionManager.RollbackAsync(cancellationToken);
            return createUserRoleResult.Errors.ToErrors();
        }

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Errors;
        }

        await _transactionManager.CommitTransactionAsync(cancellationToken);
        _logger.LogInformation("Create user email: {Email} with {Role}", email, role);

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