using Microsoft.AspNetCore.Http;
using SharedKernel;

namespace Framework.EndpointResult
{
    public sealed class SuccessResult<TValue> : IResult
    {
        private readonly TValue _value;

        public SuccessResult(TValue value)
        {
            _value = value;
        }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            var envelope = Envelope.Ok(_value);

            httpContext.Response.StatusCode = (int)StatusCodes.Status200OK;

            await httpContext.Response.WriteAsJsonAsync(envelope);
        }
    }
}
