namespace IntegrationEvents.Directory;

public static class DirectoryEventsRouting
{
    public const string EXCHANGE = "directory-events";

    public static class RoutingKeys
    {
        public const string ALL_FILE_EVENTS = "*.*.file";

        public static string DepartmentDeleted(string entityType) =>
            $"department.deleted.{entityType.ToLowerInvariant()}";
    }
}