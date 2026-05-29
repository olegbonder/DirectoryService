using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AuthService.Contracts.Dtos.Login;
using AuthService.Contracts.Dtos.RegisterUser;
using AuthService.Contracts.Dtos.UpdateRefreshToken;
using AuthService.Contracts.Dtos.VerifyEmail;
using AuthService.Domain;
using AuthService.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;
using SharedKernel.Result;

namespace AuthService.IntegrationTests.Infrastructure
{
    public class TestData
    {
        private readonly IServiceProvider _services;
        private readonly HttpClient _appHttpClient;
        private UserManager<ApplicationUser>? _userManager;

        public TestData(IServiceProvider Services, HttpClient appHttpClient)
        {
            _services = Services;
            _appHttpClient = appHttpClient;
        }

        public async Task ExecuteInUserManager(Func<UserManager<ApplicationUser>, Task> action)
        {
            await using var scope = _services.CreateAsyncScope();

            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await action(userManager);
        }

        public async Task ExecuteInDb(Func<AuthDbContext, Task> action)
        {
            await using var scope = _services.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            await action(dbContext);
        }

        public async Task<Result<Guid>> RegisterUser(
            RegisterUserRequest request,
            CancellationToken cancellationToken)
        {
            var registerUserResponse = await _appHttpClient.PostAsJsonAsync(
                Constants.REGISTER_USER_URL,
                request,
                cancellationToken);

            var registerUserResult = await registerUserResponse
                .HandleResponseAsync1<Guid>(cancellationToken);

            return registerUserResult;
        }

        public async Task<Result<Guid>> RegisterUserAndVerifyEmail(
            RegisterUserRequest request,
            CancellationToken cancellationToken)
        {
            var registerResult = await RegisterUser(request, cancellationToken);
            if (!registerResult.IsSuccess)
            {
                return registerResult;
            }

            var userId = registerResult.Value;
            var verifyEmailResult = await GenerateTokenAndVerifyEmail(userId, cancellationToken);
            if (!verifyEmailResult.IsSuccess)
            {
                return verifyEmailResult;
            }

            return userId;
        }

        public async Task<Result<LoginResponse>> Login(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var loginResponse = await _appHttpClient.PostAsJsonAsync(
                Constants.LOGIN_URL,
                request,
                cancellationToken);

            var loginResult = await loginResponse
                .HandleResponseAsync1<LoginResponse>(cancellationToken);

            return loginResult;
        }

        public async Task<Result<LoginResponse>> RefreshTokens(
            UpdateRefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            var refreshResponse = await _appHttpClient.PostAsJsonAsync(
                Constants.REFRESH_ACCESS_TOKEN_URL,
                request,
                cancellationToken);

            var refreshResult = await refreshResponse
                .HandleResponseAsync1<LoginResponse>(cancellationToken);

            return refreshResult;
        }

        public async Task<Result<Guid>> VerifyEmail(
            VerifyEmailRequest request,
            CancellationToken cancellationToken)
        {
            var queryParams = new Dictionary<string, string?>
            {
                [nameof(request.UserId)] = request.UserId.ToString(),
                [nameof(request.Token)] = request.Token
            };

            string uri = QueryHelpers.AddQueryString(Constants.CONFIRM_EMAIL_URL, queryParams);
            var confirmResponse = await _appHttpClient.GetAsync(
                uri,
                cancellationToken);

            var confirmResult = await confirmResponse
                .HandleResponseAsync1<Guid>(cancellationToken);

            return confirmResult;
        }

        public async Task<Result<Guid>> GenerateTokenAndVerifyEmail(
            Guid userId,
            CancellationToken cancellationToken)
        {
            string? confirmationToken = null;
            await ExecuteInUserManager(async userManager =>
            {
                var user = await userManager.FindByIdAsync(userId.ToString());
                confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                confirmationToken = Base64UrlEncoder.Encode(confirmationToken);
            });
            var request = new VerifyEmailRequest(userId, confirmationToken!);
            var result = await VerifyEmail(request, cancellationToken);
            return result;
        }

        public async Task<HttpResponseMessage> CallProtectedEndpoint(
            string? accessToken,
            CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Constants.PROTECTED_ENDPOINT_URL);

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await _appHttpClient.SendAsync(request, cancellationToken);
        }

        public async Task<HttpResponseMessage> CallAdminProtectedEndpoint(
            string? accessToken,
            CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Constants.ADMIN_PROTECTED_ENDPOINT_URL);

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await _appHttpClient.SendAsync(request, cancellationToken);
        }

        public string CreateFakeToken(string email, string purpose = "EmailConfirmation")
        {
            // Создаем фейковые данные токена
            var tokenData = new
            {
                Email = email,
                Purpose = purpose,
                Timestamp = DateTime.UtcNow.Ticks,
                Random = Guid.NewGuid().ToString()
            };

            // Сериализуем в JSON и кодируем
            byte[] json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(tokenData);
            return Base64UrlEncoder.Encode(json);
        }
    }

    public static class ResponseExtensions
    {
        public static async Task<Result<TResponse>> HandleResponseAsync1<TResponse>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            try
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                Envelope<object>? envelope = JsonSerializer.Deserialize<Envelope<object>>(responseContent, options);
                if (envelope is null)
                {
                    return GeneralErrors.Failure("Error while reading response");
                }

                if (!response.IsSuccessStatusCode)
                {
                    return envelope.ErrorList ?? ((Errors)Error.Failure("test.error", "Unknown error"));
                }

                if (envelope.ErrorList is not null)
                {
                    return envelope.ErrorList;
                }

                if (envelope.Result is not JsonElement jsonResult || jsonResult.ValueKind == JsonValueKind.Null)
                {
                    return GeneralErrors.Failure("Error while reading response");
                }

                var result = JsonSerializer.Deserialize<TResponse>(jsonResult.GetRawText(), options);
                return result is null ? GeneralErrors.Failure("Error while reading response") : result;
            }
            catch
            {
                return GeneralErrors.Failure("Error while reading response");
            }
        }
    }
}
