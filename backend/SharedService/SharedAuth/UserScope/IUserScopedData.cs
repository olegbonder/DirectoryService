using System.Security.Claims;

namespace SharedAuth.UserScope
{
    public interface IUserScopedData
    {
        public UserInfo Profile { get; }

        public void Authenticate(ClaimsPrincipal user);

        public bool HasPermission(string permission);
    }
}