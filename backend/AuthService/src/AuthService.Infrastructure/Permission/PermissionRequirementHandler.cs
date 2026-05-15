using AuthService.Infrastructure.UserScope;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Permission
{
    public class PermissionRequirementHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserScopedData _userScopedData;
        private readonly ILogger<PermissionRequirementHandler> _logger;

        public PermissionRequirementHandler(
            UserScopedData userScopedData,
            ILogger<PermissionRequirementHandler> logger)
        {
            _userScopedData = userScopedData;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (_userScopedData.HasPermission(requirement.Permission))
            {
                _logger.LogInformation("User has permission: {Permission}", requirement.Permission);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User does not have permission: {Permission}", requirement.Permission);
            }

            return Task.CompletedTask;
        }
    }
}