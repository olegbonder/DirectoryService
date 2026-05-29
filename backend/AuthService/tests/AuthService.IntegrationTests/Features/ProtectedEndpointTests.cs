using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Contracts.Dtos.Login;
using AuthService.Contracts.Dtos.RegisterUser;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features
{
    public class ProtectedEndpointTests : AuthServiceTestsBase
    {
        public ProtectedEndpointTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Protected_endpoint_without_token_should_return_401()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var testClient = new HttpClient();

            // act
            var response = await TestData.CallProtectedEndpoint(null, cancellationToken);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Protected_endpoint_with_invalid_token_should_return_401()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var invalidToken = "invalid.token.here";

            // act
            var response = await TestData.CallProtectedEndpoint(invalidToken, cancellationToken);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Protected_endpoint_with_valid_token_should_return_200()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "test@test.com";
            string password = "Test123!";

            var registerRequest = new RegisterUserRequest(email, password, "test", "test");
            await TestData.RegisterUserAndVerifyEmail(registerRequest, cancellationToken);

            var loginRequest = new LoginRequest(email, password);
            var loginResult = await TestData.Login(loginRequest, cancellationToken);

            var tokens = loginResult.Value;

            // act
            var response = await TestData.CallProtectedEndpoint(tokens.AccessToken, cancellationToken);

            // assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Protected_endpoint_without_required_permission_should_return_403()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "test@test.com";
            string password = "Test123!";

            var registerRequest = new RegisterUserRequest(email, password, "test", "test");
            await TestData.RegisterUserAndVerifyEmail(registerRequest, cancellationToken);

            var loginRequest = new LoginRequest(email, password);
            var loginResult = await TestData.Login(loginRequest, cancellationToken);

            var tokens = loginResult.Value;

            // act
            var response = await TestData.CallAdminProtectedEndpoint(tokens.AccessToken, cancellationToken);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}