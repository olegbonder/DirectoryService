using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedAuth.Constants;

namespace SharedAuth.DevAuth
{
    public class DevAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly DevAuthOptions _options;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DevAuthMiddleware> _logger;

        public DevAuthMiddleware(
            RequestDelegate next,
            IOptions<DevAuthOptions> options,
            IWebHostEnvironment environment,
            ILogger<DevAuthMiddleware> logger)
        {
            _next = next;
            _options = options.Value;
            _environment = environment;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == false && _options.Enabled && _environment.IsDevelopment())
            {
                _logger.LogInformation("Setting Dev Principal for request to {Path}", context.Request.Path);
                context.User = CreateDevPrincipal();
            }

            await _next(context);
        }

        private ClaimsPrincipal CreateDevPrincipal()
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, "Dev User"),
                new Claim("DevAuth", "true"),
                new Claim(ClaimTypes.Authentication, PlatformPermissions.ALL)
            };

            var identity = new ClaimsIdentity(claims, "DevAuth");
            return new ClaimsPrincipal(identity);
        }
    }
}