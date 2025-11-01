using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Shared.Result;

namespace DirectoryService.Presenters.EndpointResult
{
    public sealed class EndpointResult<TValue> : IResult, IEndpointMetadataProvider
    {
        private readonly IResult _result;

        public EndpointResult(Result<TValue> result)
        {
            _result = result.IsSuccess
                ? new SuccessResult<TValue>(result.Value)
                : new ErrorsResult(result.Error);
        }

        public static implicit operator EndpointResult<TValue>(Result<TValue> result) => new(result);

        public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(method);
            ArgumentNullException.ThrowIfNull(builder);

            builder.Metadata.Add(new ProducesResponseTypeMetadata(200, typeof(Envelope<TValue>), ["application/json"]));

            builder.Metadata.Add(new ProducesResponseTypeMetadata(400, typeof(Envelope<TValue>), ["application/json"]));
            builder.Metadata.Add(new ProducesResponseTypeMetadata(404, typeof(Envelope<TValue>), ["application/json"]));
            builder.Metadata.Add(new ProducesResponseTypeMetadata(409, typeof(Envelope<TValue>), ["application/json"]));
            builder.Metadata.Add(new ProducesResponseTypeMetadata(500, typeof(Envelope<TValue>), ["application/json"]));
        }

        public Task ExecuteAsync(HttpContext httpContext) => _result.ExecuteAsync(httpContext);
    }
}
