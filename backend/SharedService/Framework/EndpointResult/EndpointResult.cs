using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using SharedKernel;
using SharedKernel.Result;

namespace Framework.EndpointResult
{
    public sealed class EndpointResult : IResult, IEndpointMetadataProvider
    {
        private readonly IResult _result;

        public EndpointResult(Result result)
        {
            _result = result.IsSuccess
                ? new SuccessResult()
                : new ErrorsResult(result.Errors);
        }

        public static implicit operator EndpointResult(Result result) => new(result);

        public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(method);
            ArgumentNullException.ThrowIfNull(builder);

            builder.Metadata.Add(new ProducesResponseTypeMetadata(200, typeof(Envelope), ["application/json"]));

            builder.Metadata.Add(new ProducesResponseTypeMetadata(400, typeof(Envelope), ["application/json"]));
            builder.Metadata.Add(new ProducesResponseTypeMetadata(404, typeof(Envelope), ["application/json"]));
            builder.Metadata.Add(new ProducesResponseTypeMetadata(409, typeof(Envelope), ["application/json"]));
            builder.Metadata.Add(new ProducesResponseTypeMetadata(500, typeof(Envelope), ["application/json"]));
        }

        public Task ExecuteAsync(HttpContext httpContext) => _result.ExecuteAsync(httpContext);
    }
}
