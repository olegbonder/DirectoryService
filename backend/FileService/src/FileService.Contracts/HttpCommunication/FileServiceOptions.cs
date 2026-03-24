namespace FileService.Contracts.HttpCommunication;

public record FileServiceOptions
{
    public string Url { get; set; } = string.Empty;
    public int TimeoutInSeconds { get; set; } = 5;
}