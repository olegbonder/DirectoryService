using AuthService.Contracts.Dtos.RegisterUser;
using AuthService.Contracts.Dtos.VerifyEmail;
using AuthService.IntegrationTests.Infrastructure;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.IntegrationTests.Features
{
    public class VerifyEmailTests : AuthServiceTestsBase
    {
        public VerifyEmailTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task VerifyEmail_UserNotFound_ShouldReturnFailure()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var notExistUserId = Guid.NewGuid();
            string fakeToken = "fake-token-123";

            var request = new VerifyEmailRequest(notExistUserId, fakeToken);

            // act
            var result = await TestData.VerifyEmail(request, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("UserId.not.found"));
        }

        [Fact]
        public async Task ConfirmEmail_InvalidToken_ShouldReturnFailure()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "testconfirm@test.com";
            string password = "Test123!";
            string firstName = "test";
            string lastName = "test";

            // Сначала регистрируем пользователя
            var registerRequest = new RegisterUserRequest(email, password, firstName, lastName);
            var registerResult = await TestData.RegisterUser(registerRequest, cancellationToken);

            // Используем неверный токен
            string invalidToken = TestData.CreateFakeToken(email);
            var verifyRequest = new VerifyEmailRequest(registerResult.Value, invalidToken);

            // act
            var result = await TestData.VerifyEmail(verifyRequest, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("InvalidToken"));
        }

        [Fact]
        public async Task ConfirmEmail_ValidRequest_ShouldConfirmUserSuccessfully()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "testconfirmvalid@test.com";
            string password = "Test123!";
            string firstName = "test";
            string lastName = "test";

            // Регистрируем пользователя
            var registerRequest = new RegisterUserRequest(email, password, firstName, lastName);
            var registerResult = await TestData.RegisterUser(registerRequest, cancellationToken);

            // Генерируем реальный токен через UserManager
            string? confirmationToken = null;
            await TestData.ExecuteInUserManager(async userManager =>
            {
                var user = await userManager.FindByEmailAsync(email);
                Assert.NotNull(user);
                confirmationToken = Base64UrlEncoder.Encode(await userManager.GenerateEmailConfirmationTokenAsync(user));
            });

            Assert.NotNull(confirmationToken);

            var verifyRequest = new VerifyEmailRequest(registerResult.Value, confirmationToken);

            // act
            var result = await TestData.VerifyEmail(verifyRequest, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);

            // Проверяем, что email действительно подтвержден
            await TestData.ExecuteInUserManager(async userManager =>
            {
                var user = await userManager.FindByEmailAsync(email);
                Assert.NotNull(user);
                Assert.True(user.EmailConfirmed);
            });
        }

        [Fact]
        public async Task ConfirmEmail_AlreadyConfirmedUser_ShouldReturnFailure()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "testalreadyconfirmed@test.com";
            string password = "Test123!";
            string firstName = "test";
            string lastName = "test";

            // Регистрируем пользователя
            var registerRequest = new RegisterUserRequest(email, password, firstName, lastName);
            var registerResult = await TestData.RegisterUser(registerRequest, cancellationToken);

            // Подтверждаем email
            string? confirmationToken = null;
            await TestData.ExecuteInUserManager(async userManager =>
            {
                var user = await userManager.FindByEmailAsync(email);
                Assert.NotNull(user);
                confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                await userManager.ConfirmEmailAsync(user, confirmationToken);
            });

            Assert.NotNull(confirmationToken);

            // Пытаемся подтвердить снова с тем же токеном
            var verifyRequest = new VerifyEmailRequest(registerResult.Value, confirmationToken);

            // act
            var result = await TestData.VerifyEmail(verifyRequest, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("InvalidToken"));
        }
    }
}