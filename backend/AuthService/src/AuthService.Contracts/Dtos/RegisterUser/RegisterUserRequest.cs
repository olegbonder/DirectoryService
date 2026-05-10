namespace AuthService.Contracts.Dtos.RegisterUser;

public record RegisterUserRequest(string Email, string Password, string FirstName, string LastName);