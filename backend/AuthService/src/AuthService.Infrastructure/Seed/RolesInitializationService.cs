using AuthService.Domain;
using AuthService.Domain.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Result;

namespace AuthService.Infrastructure.Seed;

public class RolesInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RolesInitializationService> _logger;
    private readonly AdminOptions _options;

    public RolesInitializationService(
        IServiceProvider serviceProvider,
        ILogger<RolesInitializationService> logger,
        IOptions<AdminOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Administrator and roles initialization service started");

        await using var scope = _serviceProvider.CreateAsyncScope();

        var createRolesResult = await CreateRolesAsync(scope);
        if (createRolesResult.IsFailure)
        {
            string message = createRolesResult.Errors.First().Message;
            _logger.LogError("Error create roles {Error}", message);
            throw new ApplicationException(message);
        }

        var createAdminResult = await CreateAdminAsync(scope);
        if (createAdminResult.IsFailure)
        {
            string message = createRolesResult.Errors.First().Message;
            _logger.LogError("Error create admin {Error}", message);
            throw new ApplicationException(message);
        }

        _logger.LogInformation("Administrator and roles initialization service finished");
    }

    private async Task<Result> CreateRolesAsync(AsyncServiceScope scope)
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (string role in RolePermissions.GetRoles())
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (!createRoleResult.Succeeded)
                {
                    return Error.Failure(
                        $"migrate.create.role.{role}.failed",
                        createRoleResult.Errors.First().Description);
                }

                _logger.LogInformation("Role {Role} created", role);
            }
        }

        return Result.Success();
    }

    private async Task<Result> CreateAdminAsync(AsyncServiceScope scope)
    {
        if (string.IsNullOrWhiteSpace(_options.Email))
        {
            return Error.NotFound(
                "not.found.option.email",
                "Not found admin email in configuration options");
        }

        if (string.IsNullOrWhiteSpace(_options.Password))
        {
            return Error.NotFound(
                "not.found.option.password",
                "Not found admin password in configuration options");
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(_options.Email);
        if (user == null)
        {
            string userName = _options.UserName;
            var createAdminResult = ApplicationUser.Create(
                _options.Email,
                userName,
                userName,
                userName);
            if (createAdminResult.IsFailure)
            {
                return createAdminResult.Errors;
            }

            user = createAdminResult.Value;
            var createUserResult = await userManager.CreateAsync(user);
            if (!createUserResult.Succeeded)
            {
                return Error.Failure(
                    $"create.user.{user.FirstName}.failed",
                    createUserResult.Errors.First().Description);
            }

            string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmEmailResult = await userManager.ConfirmEmailAsync(user, token);
            if (!confirmEmailResult.Succeeded)
            {
                return Error.Failure(
                    $"confirm.user.{user.FirstName}.email.failed",
                    createUserResult.Errors.First().Description);
            }
        }

        string adminRole = PlatformGroups.ADMIN;
        if (!await userManager.IsInRoleAsync(user, adminRole))
        {
            var createRoleResult = await userManager.AddToRoleAsync(user, adminRole);
            if (!createRoleResult.Succeeded)
            {
                return Error.Failure(
                    $"create.user.role.{adminRole}.failed",
                    createRoleResult.Errors.First().Description);
            }

            _logger.LogInformation("Role {Role} created for {User}", adminRole, user.FirstName);
        }

        return Result.Success();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}