namespace AuthService.Domain.Permissions;

public static class RolePermissions
{
    private static readonly Dictionary<string, HashSet<string>> _mapping = new(StringComparer.OrdinalIgnoreCase)
    {
        [PlatformGroups.PARTICIPANT] = [PlatformPermissions.CONTENT_VIEW],
        [PlatformGroups.AUTHOR] =
        [
            PlatformPermissions.CONTENT_VIEW,
            PlatformPermissions.CONTENT_MANAGE,
            PlatformPermissions.FILES_MANAGE
        ],
        [PlatformGroups.MODERATOR] =
        [
            PlatformPermissions.CONTENT_VIEW,
            PlatformPermissions.CONTENT_MANAGE,
            PlatformPermissions.FILES_MANAGE,
            PlatformPermissions.USERS_MANAGE
        ],
        [PlatformGroups.ADMIN] = [PlatformPermissions.ALL]
    };

    public static IEnumerable<string> GetRoles() => _mapping.Keys;

    public static HashSet<string> GetPermissions(IEnumerable<string> groups)
    {
        HashSet<string> permissions = [];

        foreach (string group in groups)
        {
            if (_mapping.TryGetValue(group, out HashSet<string>? rolePerms))
            {
                permissions.UnionWith(rolePerms);
            }
        }

        return permissions;
    }
}