using AuthService.Domain.Shared;
using AuthService.Domain.Token;
using Microsoft.AspNetCore.Identity;
using SharedKernel.Result;

namespace AuthService.Domain
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        private List<RefreshToken> _refreshTokens = [];

        // EF Core
        private ApplicationUser()
        {
        }

        public string FirstName { get; init; } = null!;

        public string LastName { get; init; } = null!;

        public DateTime CreatedAt { get; init; }

        public bool IsActive { get; init; }

        public List<RefreshToken> RefreshTokens => _refreshTokens;

        private ApplicationUser(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public static Result<ApplicationUser> Create(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                return UserErrors.FirstNameIsEmpty();
            }

            const int min = LengthConstants.LENGTH_2;
            const int max = LengthConstants.LENGTH_100;

            if (firstName.Length < min || firstName.Length > max)
            {
                return UserErrors.FirstNameLengthOutOfRange(min, max);
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                return UserErrors.LastNameIsEmpty();
            }

            if (lastName.Length < min || lastName.Length > max)
            {
                return UserErrors.LastNameLengthOutOfRange(min, max);
            }

            return new ApplicationUser(firstName, lastName);
        }
    }
}