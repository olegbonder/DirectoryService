using System.Security.Claims;
using AuthService.Domain;

namespace AuthService.Application
{
    public interface IUserScopedData
    {
        public UserInfo Profile { get; }

        public void Authenticate(ClaimsPrincipal user);

        public bool HasPermission(string permission);
    }
}