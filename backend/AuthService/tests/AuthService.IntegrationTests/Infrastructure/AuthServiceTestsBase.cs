using AuthService.Domain;
using AuthService.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SharedAuth.Constants;

namespace AuthService.IntegrationTests.Infrastructure;

public class AuthServiceTestsBase : IAsyncLifetime, IClassFixture<IntegrationTestsWebFactory>
{
    private readonly IntegrationTestsWebFactory _factory;

    protected AuthServiceTestsBase(IntegrationTestsWebFactory factory)
    {
        _factory = factory;
        AppHttpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        Services = _factory.Services;
        TestData = new TestData(Services, AppHttpClient);
    }

    protected IServiceProvider Services { get; init; }

    protected HttpClient AppHttpClient { get; init; }

    protected TestData TestData { get; init; }

    public async Task InitializeAsync()
    {
        await CreateRolesAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    private async Task CreateRolesAsync()
    {
        var roleManager = Services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (string role in RolePermissions.GetRoles())
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}