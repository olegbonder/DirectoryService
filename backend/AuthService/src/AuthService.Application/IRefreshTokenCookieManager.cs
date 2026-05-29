namespace AuthService.Application;

public interface IRefreshTokenCookieManager
{
    void Set(string refreshToken);

    void Delete();

    string? Get();
}