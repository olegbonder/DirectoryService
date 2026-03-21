using System.Net.Http.Json;
using SharedKernel;
using SharedKernel.Result;

namespace FileService.Core.HttpCommunication;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        try
        {
            Envelope<TResponse>? startMultiPartResponse = await response.Content
            .ReadFromJsonAsync<Envelope<TResponse>?>(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return startMultiPartResponse?.ErrorList
                       ?? GeneralErrors.Failure("Error while reading response");
            }

            if (startMultiPartResponse is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            if (startMultiPartResponse.ErrorList is not null)
            {
                return startMultiPartResponse.ErrorList;
            }

            if (startMultiPartResponse.Result is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            return startMultiPartResponse.Result;
        }
        catch(Exception ex)
        {
            throw;
            //return GeneralErrors.Failure("Error while reading response");
        }
    }

    public static async Task<Result> HandleResponseAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Envelope? startMultiPartResponse = await response.Content
                .ReadFromJsonAsync<Envelope>(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return startMultiPartResponse?.ErrorList
                ?? GeneralErrors.Failure("Error while reading response");
            }

            if (startMultiPartResponse is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            if (startMultiPartResponse.ErrorList is not null)
            {
                return startMultiPartResponse.ErrorList;
            }

            return Result.Success();
        }
        catch
        {
            return GeneralErrors.Failure("Error while reading response");
        }
    }
}