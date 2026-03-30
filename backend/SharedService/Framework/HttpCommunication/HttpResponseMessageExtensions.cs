using System.Net.Http.Json;
using SharedKernel;
using SharedKernel.Result;

namespace Framework.HttpCommunication
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<Result<TResponse>> HandleResponseAsync<TResponse>(
            this HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            try
            {
                Envelope<TResponse>? envelope = await response.Content
                    .ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return envelope?.ErrorList ?? Error.Failure("test.error", "Unknown error");
                }

                if (envelope is null)
                {
                    return GeneralErrors.Failure("Error while reading response");
                }

                if (envelope.Result is null)
                {
                    return GeneralErrors.Failure("Error while reading response");
                }

                if (envelope.ErrorList is not null)
                {
                    return envelope.ErrorList;
                }

                return envelope.Result;
            }
            catch
            {
                return GeneralErrors.Failure("Error while reading response");
            }
        }

        public static async Task<Result<TResponse?>> HandleNullableResponseAsync<TResponse>(
            this HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            try
            {
                Envelope<TResponse>? envelope = await response.Content
                    .ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return envelope?.ErrorList ?? Error.Failure("test.error", "Unknown error");
                }

                if (envelope is null)
                {
                    return GeneralErrors.Failure("Error while reading response");
                }

                return envelope.Result;
            }
            catch
            {
                return GeneralErrors.Failure("Error while reading response");
            }
        }

        public static async Task<Result> HandleResponseAsync(
            this HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            try
            {
                Envelope? envelope = await response.Content
                    .ReadFromJsonAsync<Envelope>(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return envelope?.ErrorList ?? Error.Failure("test.error", "Unknown error");
                }

                if (envelope is null)
                {
                    return GeneralErrors.Failure("Error while reading response");
                }

                if (envelope.ErrorList is not null)
                {
                    return envelope.ErrorList;
                }

                return Result.Success();
            }
            catch
            {
                return GeneralErrors.Failure("Error while reading response");
            }
        }
    }
}