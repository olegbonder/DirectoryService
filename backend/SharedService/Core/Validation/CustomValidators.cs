using System.Text.Json;
using FluentValidation;
using SharedKernel.Result;

namespace Core.Validation
{
    public static class CustomValidators
    {
        public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
            this IRuleBuilder<T, TElement> ruleBuilder,
            Func<TElement, Result<TValueObject>> factoryMethod)
        {
            return ruleBuilder.Custom((value, context) =>
            {
                Result<TValueObject> result = factoryMethod(value);

                if (result.IsSuccess)
                {
                    return;
                }

                context.AddFailure(JsonSerializer.Serialize(result.Errors));
            });
        }

        public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Error error)
        {
            return rule.WithMessage(JsonSerializer.Serialize(error.ToErrors()));
        }
    }
}
