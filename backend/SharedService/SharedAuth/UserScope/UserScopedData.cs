using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SharedAuth.Constants;

namespace SharedAuth.UserScope
{
    public class UserScopedData : IUserScopedData
    {
        public UserInfo Profile { get; set; } = null!;

        public void Authenticate(ClaimsPrincipal user)
        {
            var userIdStr = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var tryParseUserId = Guid.TryParse(userIdStr, out var userId);
            var email = user.FindFirstValue(JwtRegisteredClaimNames.Email);

            var name = user.FindFirstValue(JwtRegisteredClaimNames.Name);
            var firstName = string.Empty;
            var lastName = string.Empty;
            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameParts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                firstName = nameParts.FirstOrDefault() ?? string.Empty;
                lastName = nameParts.Length > 1 ? nameParts.Skip(1).FirstOrDefault() : string.Empty;
            }

            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = user.FindAll(ClaimTypes.Authentication).Select(c => c.Value).ToList();
            Profile = new UserInfo(userId, email!, firstName, lastName!, roles, permissions);
        }

        public bool HasPermission(string permission) =>
            Profile is not null &&
            (Profile.Permissions.Contains(PlatformPermissions.ALL) || Profile.Permissions.Contains(permission));
    }
}