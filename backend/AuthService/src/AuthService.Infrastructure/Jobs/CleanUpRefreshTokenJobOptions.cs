namespace AuthService.Infrastructure.Jobs;

public class CleanUpRefreshTokenJobOptions
{
    public const string SECTION_NAME = "CleanUpRefreshTokens";

    public int IntervalInDays { get; init; } = 1;

    public int RevokeTokenOlderThanDays { get; init; } = 30;
}