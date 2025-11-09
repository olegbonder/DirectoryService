using FluentValidation.Results;
using Shared.Result;

namespace DirectoryService.Application.Validation
{
    public static class ValidationExtensions
    {
        public static Errors ToList(this ValidationResult validationResult)
        {
            var validationErrors = validationResult.Errors;

            IEnumerable<Error> errors = from validationError in validationErrors
                         let errorMessage = validationError.ErrorMessage
                         let error = Error.Deserialize(errorMessage)
                         select Error.Validation(error.Code, error.Message, validationError.PropertyName);

            return errors.ToArray();
        }
    }
}
