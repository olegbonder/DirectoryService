using SharedKernel.Result;

namespace AuthService.Domain.Shared;

public static class UserErrors
{
    public static Error EmailIsEmpty()
    {
        return GeneralErrors.PropertyIsEmpty("user.email");
    }

    public static Error UserNameIsEmpty()
    {
        return GeneralErrors.PropertyIsEmpty("user.login");
    }

    public static Error FirstNameIsEmpty()
    {
        return GeneralErrors.PropertyIsEmpty("user.firstName");
    }

    public static Error FirstNameLengthOutOfRange(int min, int max)
    {
        return GeneralErrors.PropertyOutOfRange("user.firstName", min, max);
    }

    public static Error LastNameIsEmpty()
    {
        return GeneralErrors.PropertyIsEmpty("user.lastName");
    }

    public static Error LastNameLengthOutOfRange(int min, int max)
    {
        return GeneralErrors.PropertyOutOfRange("user.lastName", min, max);
    }

    public static Error FailedLoginOrPassword()
    {
        return Error.Validation("user.failed.login.or.password", "Failed login or password");
    }

    public static Error InActiveUser()
    {
        return Error.Failure("user.not.active", "User is not active");
    }

    public static Error UserEmailNotConfirmed()
    {
        return Error.Failure("user.email.not.confirmed", "User account is not confirmed");
    }

    public static Error UserLockedOut(DateTimeOffset? lockoutEndDate)
    {
        return Error.Failure("user.lockout", "User is locked-out at " + lockoutEndDate);
    }
}