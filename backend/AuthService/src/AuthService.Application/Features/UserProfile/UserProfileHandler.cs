using AuthService.Domain;
using Core.Abstractions;
using Microsoft.Extensions.Logging;
using SharedAuth;
using SharedAuth.UserScope;
using SharedKernel.Result;

namespace AuthService.Application.Features.UserProfile
{
    public class UserProfileHandler : IQueryHandler<UserInfo>
    {
        private readonly IUserScopedData _userScopedData;
        private readonly ILogger<UserProfileHandler> _logger;

        public UserProfileHandler(IUserScopedData userScopedData, ILogger<UserProfileHandler> logger)
        {
            _userScopedData = userScopedData;
            _logger = logger;
        }

        public Task<Result<UserInfo>> Handle(CancellationToken cancellationToken)
        {
            var profile = _userScopedData.Profile;
            var result = new UserInfo(
                profile.Id,
                profile.Email,
                profile.FirstName,
                profile.LastName,
                profile.Roles,
                profile.Permissions);

            _logger.LogInformation("Get user profile {UserId} success", profile.Id);

            return Task.FromResult(Result<UserInfo>.Success(result));
        }
    }
}