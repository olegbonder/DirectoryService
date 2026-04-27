namespace DirectoryService.Contracts;

public record MediaDto
{
    public Guid Id { get; init; }
    public string? Url { get; init; }
    public string Status { get; init; } = string.Empty;
}