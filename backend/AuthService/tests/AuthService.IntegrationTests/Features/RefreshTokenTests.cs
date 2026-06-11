using System.Net.Http.Json;
using AuthService.Contracts.Dtos.Login;
using AuthService.Contracts.Dtos.RegisterUser;
using AuthService.Contracts.Dtos.UpdateRefreshToken;
using AuthService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AuthService.IntegrationTests.Features
{
    public class RefreshTokenTests : AuthServiceTestsBase
    {
        public RefreshTokenTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Refresh_with_valid_tokens_should_return_new_tokens()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "test@test.com";
            string password = "Test123!";

            var registerRequest = new RegisterUserRequest(email, password, "test", "test");
            var registerResult = await TestData.RegisterUserAndVerifyEmail(registerRequest, cancellationToken);
            var userId = registerResult.Value;
            var loginRequest = new LoginRequest(email, password);
            var loginResult = await TestData.Login(loginRequest, cancellationToken);

            var initialTokens = loginResult.Value;
            string initialRefreshToken = string.Empty;
            await TestData.ExecuteInDb(async db =>
            {
                var refreshTokenEntity = await db.RefreshTokens.FirstOrDefaultAsync(
                    r => r.UserId == userId && r.ReplacedByToken == null,
                    cancellationToken);
                Assert.NotNull(refreshTokenEntity);
                initialRefreshToken = refreshTokenEntity!.Token.Value;
            });

            var refreshRequest = new UpdateRefreshTokenRequest(initialTokens.AccessToken);

            // act
            var updateRefreshTokenResponseMessage = await AppHttpClient.PostAsJsonAsync(Constants.REFRESH_ACCESS_TOKEN_URL, refreshRequest, cancellationToken);
            var result = await updateRefreshTokenResponseMessage.HandleResponseAsync1<LoginResponse>(cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            var newTokens = result.Value;
            Assert.NotNull(newTokens.AccessToken);
            Assert.True(newTokens.ExpiresIn > 0);
            Assert.True(updateRefreshTokenResponseMessage.Headers.TryGetValues("Set-Cookie", out var setCookieValues));
            Assert.Contains(setCookieValues, header => header.Contains("refreshToken=", StringComparison.OrdinalIgnoreCase));

            // new tokens should be different from old ones
            Assert.NotEqual(initialTokens.AccessToken, newTokens.AccessToken);

            await TestData.ExecuteInDb(async db =>
            {
                var oldRefreshToken = await db.RefreshTokens.FirstOrDefaultAsync(
                    r =>
                    r.UserId == userId && r.Token.Value == initialRefreshToken, cancellationToken);
                var newRefreshToken = await db.RefreshTokens.FirstOrDefaultAsync(
                    r =>
                        r.UserId == userId && r.ReplacedByToken == null, cancellationToken);
                Assert.NotNull(oldRefreshToken);
                Assert.NotNull(newRefreshToken);
                Assert.Equal(oldRefreshToken.ReplacedByToken, newRefreshToken.Token.Value);
                Assert.NotNull(oldRefreshToken.RevokedAt);
            });
        }

        [Fact]
        public async Task Refresh_with_expired_refresh_token_should_fail()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string expiredRefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1MTYyMzkwMjJ9.expired_token";
            string validAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";

            var refreshRequest = new UpdateRefreshTokenRequest(validAccessToken);

            // act
            var result = await TestData.RefreshTokens(refreshRequest, expiredRefreshToken, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Refresh_with_already_used_refresh_token_should_fail()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "test@test.com";
            string password = "Test123!";

            var registerRequest = new RegisterUserRequest(email, password, "test", "test");
            var registerResult = await TestData.RegisterUserAndVerifyEmail(registerRequest, cancellationToken);
            var userId = registerResult.Value;

            var loginRequest = new LoginRequest(email, password);
            var loginResult = await TestData.Login(loginRequest, cancellationToken);

            var initialTokens = loginResult.Value;
            string initialRefreshToken = string.Empty;
            await TestData.ExecuteInDb(async db =>
            {
                var refreshTokenEntity = await db.RefreshTokens.FirstOrDefaultAsync(
                    r => r.UserId == userId && r.ReplacedByToken == null,
                    cancellationToken);
                Assert.NotNull(refreshTokenEntity);
                initialRefreshToken = refreshTokenEntity!.Token.Value;
            });

            var refreshRequest = new UpdateRefreshTokenRequest(initialTokens.AccessToken);

            // use the refresh token once
            var firstRefreshResponse = await AppHttpClient.PostAsJsonAsync(Constants.REFRESH_ACCESS_TOKEN_URL, refreshRequest, cancellationToken);
            var firstRefreshResult = await firstRefreshResponse.HandleResponseAsync1<LoginResponse>(cancellationToken);
            var firstTokens = firstRefreshResult.Value;

            firstRefreshResponse.Headers.TryGetValues("Set-Cookie", out var setCookieFirstRefreshValues);
            var refreshTokenCookieFirstRefresh = setCookieFirstRefreshValues.First().Split(";")[0];

            // try to use the same refresh token again
            var secondRefreshResponse = await AppHttpClient.PostAsJsonAsync(Constants.REFRESH_ACCESS_TOKEN_URL, refreshRequest, cancellationToken);
            var secondRefreshResult = await secondRefreshResponse.HandleResponseAsync1<LoginResponse>(cancellationToken);

            // assert
            Assert.True(secondRefreshResult.IsSuccess);
            var secondTokens = secondRefreshResult.Value;
            Assert.NotNull(secondTokens.AccessToken);
            Assert.True(secondTokens.ExpiresIn > 0);
            Assert.NotEqual(firstTokens.AccessToken, secondTokens.AccessToken);
            Assert.True(secondRefreshResponse.Headers.TryGetValues("Set-Cookie", out var setCookieSecondRefreshValues));
            Assert.Contains(setCookieSecondRefreshValues, header => header.Contains("refreshToken=", StringComparison.OrdinalIgnoreCase));
            var refreshTokenCookieSecondRefresh = setCookieSecondRefreshValues.First().Split(";")[0];
            Assert.NotEqual(refreshTokenCookieFirstRefresh, refreshTokenCookieSecondRefresh);
        }
    }
}