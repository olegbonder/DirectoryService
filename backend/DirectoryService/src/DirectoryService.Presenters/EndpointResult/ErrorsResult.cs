using Microsoft.AspNetCore.Http;
using Shared.Result;

namespace DirectoryService.Presenters.EndpointResult
{
    public sealed class ErrorsResult: IResult
    {
        private readonly Errors _errors;

        public ErrorsResult(Error error)
        {
            _errors = error.ToErrors();
        }

        public ErrorsResult(Errors errors)
        {
            _errors = errors;
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            if (!_errors.Any())
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                return httpContext.Response.WriteAsJsonAsync(Envelope.Error(_errors));
            }

            var distinctErrorTypes = _errors.Select(e => e.Type).Distinct().ToList();

            int statusCode = distinctErrorTypes.Count() > 1
                ? StatusCodes.Status500InternalServerError
                : GetStatusCodeForErrorType(distinctErrorTypes.First());
            var envelope = Envelope.Error(_errors);

            httpContext.Response.StatusCode = statusCode;

            return httpContext.Response.WriteAsJsonAsync(envelope);
        }

        private static int GetStatusCodeForErrorType(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Failure => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };
    }
}