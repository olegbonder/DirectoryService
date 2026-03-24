using System.Net.Http.Json;
using SharedKernel;
using SharedKernel.Result;

namespace FileService.Contracts;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        try
        {
            Envelope<TResponse>? jsonResponse = await response.Content
            .ReadFromJsonAsync<Envelope<TResponse>?>(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return jsonResponse?.ErrorList ?? GeneralErrors.Failure("Error while reading response");
            }

            if (jsonResponse is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            if (jsonResponse.ErrorList is not null)
            {
                return jsonResponse.ErrorList;
            }

            if (jsonResponse.Result is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            return jsonResponse.Result;
        }
        catch
        {
            return GeneralErrors.Failure("Error while reading response");
        }
    }

    public static async Task<Result> HandleResponseAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Envelope? jsonResult = await response.Content
                .ReadFromJsonAsync<Envelope>(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return jsonResult?.ErrorList ?? GeneralErrors.Failure("Error while reading response");
            }

            if (jsonResult is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            if (jsonResult.ErrorList is not null)
            {
                return jsonResult.ErrorList;
            }

            return Result.Success();
        }
        catch
        {
            return GeneralErrors.Failure("Error while reading response");
        }
    }
}