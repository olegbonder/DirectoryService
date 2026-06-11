using AuthService.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedAuth.Jwt;

namespace AuthService.Infrastructure;

public class RefreshTokenCookieManager : IRefreshTokenCookieManager
{
    private const string COOKIE_NAME = "refreshToken";
    public const string COOKIE_PATH = "/api/auth";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RefreshTokenCookieManager> _logger;
    private readonly JwtOptions _options;
    private readonly bool _isProduction;

    public RefreshTokenCookieManager(
        IHttpContextAccessor httpContextAccessor,
        IOptions<JwtOptions> options,
        ILogger<RefreshTokenCookieManager> logger)
    {
        _options = options.Value;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        _isProduction = environmentName == "Release";
    }

    public void Set(string refreshToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        context.Response.Cookies.Append(COOKIE_NAME, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = _isProduction,
            SameSite = SameSiteMode.Strict,
            Path = COOKIE_PATH,
            MaxAge = TimeSpan.FromDays(_options.RefreshTokenExpirationDays),
            IsEssential = true
        });

        _logger.LogInformation("Refresh token cookie set for path: {Path}", COOKIE_PATH);
    }

    public void Delete()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        context.Response.Cookies.Delete(COOKIE_NAME, new CookieOptions
        {
            HttpOnly = true,
            Secure = _isProduction,
            SameSite = SameSiteMode.Strict,
            Path = COOKIE_PATH
        });

        _logger.LogInformation("Refresh token cookie deleted");
    }

    public string? Get()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.Request.Cookies[COOKIE_NAME];
    }
}