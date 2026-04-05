namespace IntegrationEvents.Files.Events;

public record VideoDeleted(
    Guid VideoId,
    Guid EntityId,
    string EntityType);