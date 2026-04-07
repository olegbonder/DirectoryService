namespace IntegrationEvents.Files.Events;

public record VideoCreated(
    Guid VideoId,
    Guid EntityId,
    string EntityType);