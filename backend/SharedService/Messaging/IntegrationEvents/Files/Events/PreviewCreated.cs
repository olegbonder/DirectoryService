namespace IntegrationEvents.Files.Events;

public record PreviewCreated(
    Guid PreviewId,
    Guid EntityId,
    string EntityType);