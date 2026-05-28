using AuthService.Contracts.Dtos.RegisterUser;
using AuthService.Domain;
using AuthService.IntegrationTests.Infrastructure;
using SharedAuth.Constants;

namespace AuthService.IntegrationTests.Features
{
    public class RegisterUserTests : AuthServiceTestsBase
    {
        public RegisterUserTests(IntegrationTestsWebFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task RegisterUser_with_valid_request_should_success()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "test@test.com";
            string password = "Test123!";
            string firstName = "test";
            string lastName = "test";

            var request = new RegisterUserRequest(email, password, firstName, lastName);

            // act
            var result = await TestData.RegisterUser(request, cancellationToken);

            // assert
            Assert.True(result.IsSuccess);
            var userId = result.Value;
            await TestData.ExecuteInUserManager(async userManager =>
            {
                ApplicationUser? user = await userManager.FindByIdAsync(userId.ToString());
                Assert.NotNull(user);
                Assert.Equal(email, user.Email);
                Assert.Equal(firstName, user.FirstName);
                Assert.Equal(lastName, user.LastName);

                var userRoles = await userManager.GetRolesAsync(user);
                Assert.Single(userRoles);
                Assert.Contains(userRoles, r => r == PlatformGroups.PARTICIPANT);
            });
        }

        [Fact]
        public async Task RegisterUser_with_invalid_request_should_conflict_email()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "test@test.com";
            string password = "Test123!";

            var firstRequest = new RegisterUserRequest(email, password, "test", "test");
            var secondRequest = new RegisterUserRequest(email, password, "test1", "test1");

            // act
            await TestData.RegisterUser(firstRequest, cancellationToken);
            var result = await TestData.RegisterUser(secondRequest, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(2, result.Errors.Count());

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("DuplicateUserName"));
            Assert.Contains(errorCodes, e => e.Contains("DuplicateEmail"));
        }

        [Fact]
        public async Task RegisterUser_with_invalid_request_should_simple_password()
        {
            // arrange
            var cancellationToken = new CancellationTokenSource().Token;
            string email = "test@test.com";
            string password = "Test12";
            string firstName = "test";
            string lastName = "test";

            var request = new RegisterUserRequest(email, password, firstName, lastName);

            // act
            var result = await TestData.RegisterUser(request, cancellationToken);

            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(2, result.Errors.Count());

            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(errorCodes, e => e.Contains("user.password.min_length"));
            Assert.Contains(errorCodes, e => e.Contains("user.password.special_character"));
        }
    }
}