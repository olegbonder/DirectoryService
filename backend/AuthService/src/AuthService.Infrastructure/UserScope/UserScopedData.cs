using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthService.Infrastructure.UserScope
{
    public class UserScopedData
    {
        public int UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public IEnumerable<string> Roles { get; set; } = [];

        public IEnumerable<string> Permissions { get; set; } = [];

        public void Authenticate(ClaimsPrincipal user)
        {
            var tryParseUserId = int.TryParse(user.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId);
            UserId = userId;
            Email = user.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
            Name = user.FindFirstValue(JwtRegisteredClaimNames.Name) ?? string.Empty;
            Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            Permissions = user.FindAll(ClaimTypes.Authentication).Select(c => c.Value).ToList();
        }

        public bool HasPermission(string permission) => Permissions.Contains(permission);
    }
}