namespace AuthService.Application.Permission
{
    public static class EndpointPermissionExtensions
    {
        public static TBuilder RequirePermissions<TBuilder>(this TBuilder builder, params string[] permissions)
        {
            if (permissions is null || permissions.Length == 0)
            {
                return builder;
            }

            var validPermissions = permissions.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();

            foreach (var permission in validPermissions)
            {
                builder.RequirePermissions(permission);
            }

            return builder;
        }
    }
}