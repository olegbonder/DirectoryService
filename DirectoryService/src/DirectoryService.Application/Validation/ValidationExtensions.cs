using System.Text.Json;
using FluentValidation.Results;
using Shared.Result;

namespace DirectoryService.Application.Validation
{
    public static class ValidationExtensions
    {
        public static Errors ToList(this ValidationResult validationResult)
        {
            var validationErrors = validationResult.Errors;

            IEnumerable<IEnumerable<Error>> errors = from validationError in validationErrors
                         let errorMessage = validationError.ErrorMessage
                         let error = JsonSerializer.Deserialize<IEnumerable<Error>>(errorMessage)
                         select error;

            return new Errors(errors.SelectMany(e => e));
        }
    }
}
