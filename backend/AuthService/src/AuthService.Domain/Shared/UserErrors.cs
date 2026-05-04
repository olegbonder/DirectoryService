using SharedKernel.Result;

namespace AuthService.Domain.Shared;

public static class UserErrors
{
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
}