using Microsoft.AspNetCore.Identity;
using SharedKernel.Result;

namespace AuthService.Domain.Shared;

public static class IdentityErrorExtensions
{
    public static Errors ToErrors(this IEnumerable<IdentityError> identityErrors)
    {
        var errors = identityErrors.Select(e => Error.Failure(e.Code, e.Description));
        return new Errors(errors);
    }

    public static string ToErrorString(this IEnumerable<IdentityError> identityErrors)
    {
        var messages = string.Join("; ", identityErrors.Select(e => $"{e.Code}: {e.Description}"));
        return messages;
    }
}