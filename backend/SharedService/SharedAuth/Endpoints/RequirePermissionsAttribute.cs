using Microsoft.AspNetCore.Authorization;

namespace SharedAuth.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionsAttribute : AuthorizeAttribute
    {
        public RequirePermissionsAttribute(params string[] permissions)
        {
            if (permissions == null || permissions.Length == 0)
                return;

            var validPermissions = permissions
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            if (validPermissions.Length > 0)
            {
                Policy = string.Join(",", validPermissions);
            }
        }
    }
}