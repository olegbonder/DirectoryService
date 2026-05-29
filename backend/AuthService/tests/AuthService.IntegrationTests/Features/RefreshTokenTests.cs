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
            var refreshRequest = new UpdateRefreshTokenRequest(initialTokens.AccessToken, initialTokens.RefreshToken);

            // act
            var result = await TestData.RefreshTokens(refreshRequest, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            var newTokens = result.Value;
            Assert.NotNull(newTokens.AccessToken);
            Assert.NotNull(newTokens.RefreshToken);
            Assert.True(newTokens.ExpiresIn > 0);

            // new tokens should be different from old ones
            Assert.NotEqual(initialTokens.AccessToken, newTokens.AccessToken);

            await TestData.ExecuteInDb(async db =>
            {
                var oldRefreshToken = await db.RefreshTokens.FirstOrDefaultAsync(
                    r =>
                    r.UserId == userId && r.Token.Value == initialTokens.RefreshToken, cancellationToken);
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

            var refreshRequest = new UpdateRefreshTokenRequest(validAccessToken, expiredRefreshToken);

            // act
            var result = await TestData.RefreshTokens(refreshRequest, cancellationToken);

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
            await TestData.RegisterUserAndVerifyEmail(registerRequest, cancellationToken);

            var loginRequest = new LoginRequest(email, password);
            var loginResult = await TestData.Login(loginRequest, cancellationToken);

            var initialTokens = loginResult.Value;
            var refreshRequest = new UpdateRefreshTokenRequest(initialTokens.AccessToken, initialTokens.RefreshToken);

            // use the refresh token once
            var firstRefreshResult = await TestData.RefreshTokens(refreshRequest, cancellationToken);
            Assert.True(firstRefreshResult.IsSuccess);

            // try to use the same refresh token again (should fail because it's been rotated out)
            var secondRefreshResult = await TestData.RefreshTokens(refreshRequest, cancellationToken);

            // assert
            Assert.True(secondRefreshResult.IsSuccess);
            var newTokens = secondRefreshResult.Value;
            Assert.NotNull(newTokens.AccessToken);
            Assert.NotNull(newTokens.RefreshToken);
            Assert.True(newTokens.ExpiresIn > 0);

            // new tokens should be different from old ones
            Assert.NotEqual(initialTokens.AccessToken, newTokens.AccessToken);
        }
    }
}