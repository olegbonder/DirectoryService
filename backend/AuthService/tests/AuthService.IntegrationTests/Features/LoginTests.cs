using AuthService.Contracts.Dtos.Login;
using AuthService.Contracts.Dtos.RegisterUser;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features
{
    public class LoginTests : AuthServiceTestsBase
    {
        public LoginTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Login_with_valid_credentials_after_email_confirmation_should_return_tokens()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "test@test.com";
            string password = "Test123!";

            var registerRequest = new RegisterUserRequest(email, password, "test", "test");
            await TestData.RegisterUserAndVerifyEmail(registerRequest, cancellationToken);

            var loginRequest = new LoginRequest(email, password);

            // act
            var result = await TestData.Login(loginRequest, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            var loginResponse = result.Value;
            Assert.NotNull(loginResponse.AccessToken);
            Assert.NotNull(loginResponse.RefreshToken);
            Assert.True(loginResponse.ExpiresIn > 0);
        }

        [Fact]
        public async Task Login_with_unconfirmed_email_should_fail()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "unconfirmed@test.com";
            string password = "Test123!";

            var registerRequest = new RegisterUserRequest(email, password, "test", "test");
            await TestData.RegisterUser(registerRequest, cancellationToken);

            // do NOT confirm email
            var loginRequest = new LoginRequest(email, password);

            // act
            var result = await TestData.Login(loginRequest, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("user.email.not.confirmed"));
        }

        [Fact]
        public async Task Login_with_wrong_password_should_fail()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "test@test.com";
            string password = "Test123!";
            string wrongPassword = "WrongPassword123!";

            var registerRequest = new RegisterUserRequest(email, password, "test", "test");
            await TestData.RegisterUserAndVerifyEmail(registerRequest, cancellationToken);

            var loginRequest = new LoginRequest(email, wrongPassword);

            // act
            var result = await TestData.Login(loginRequest, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("user.failed.login.or.password"));
        }

        [Fact]
        public async Task Login_with_multiple_failed_attempts_should_trigger_lockout()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "lockout@test.com";
            string password = "Test123!";
            string wrongPassword = "WrongPassword123!";

            var registerRequest = new RegisterUserRequest(email, password, "test", "test");
            await TestData.RegisterUserAndVerifyEmail(registerRequest, cancellationToken);

            // attempt login with wrong password 5 times (the configured max failed attempts)
            for (int i = 0; i < 6; i++)
            {
                var failedLoginRequest = new LoginRequest(email, wrongPassword);
                var failedResult = await TestData.Login(failedLoginRequest, cancellationToken);
                Assert.True(failedResult.IsFailure);
            }

            // attempt login with correct password should still fail due to lockout
            var loginRequest = new LoginRequest(email, password);

            // act
            var result = await TestData.Login(loginRequest, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("user.lockout"));
        }
    }
}