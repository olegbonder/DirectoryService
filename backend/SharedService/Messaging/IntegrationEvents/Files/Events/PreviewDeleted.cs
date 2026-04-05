namespace IntegrationEvents.Files.Events;

public record PreviewDeleted(
    Guid PreviewId,
    Guid EntityId,
    string EntityType);