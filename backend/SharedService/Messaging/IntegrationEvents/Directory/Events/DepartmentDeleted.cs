namespace IntegrationEvents.Directory.Events
{
    public record DepartmentDeleted(Guid MediaAssetId, string Context, Guid EntityId);
}