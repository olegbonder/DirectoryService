using System.Security.Cryptography;

namespace AuthService.Domain.Token;

public sealed record Base64Token
{
    private Base64Token(string value)
    {
        Value = value;
    }

    public string Value { get; set; }

    private static string GenerateToken(int bytes = 32)
    {
        byte[] randomBytes = new byte[bytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return Convert.ToBase64String(randomBytes);
    }

    public static Base64Token Create()
    {
        string newToken = GenerateToken();
        return new Base64Token(newToken);
    }
}