using System.Net.Http.Json;
using System.Text.Json;
using SharedKernel.Result;

namespace FileService.Core.HttpCommunication;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            /*var options = new JsonSerializerOptions
            {
                Converters = { new ErrorsJsonConverter() }
            };*/
            var envelope = await response.Content
                .ReadFromJsonAsync<Envelope<TResponse>?>(cancellationToken);

            /*if (!response.IsSuccessStatusCode)
            {
                return envelope != null && envelope.ErrorList != null
                    ? Result<TResponse>.Failure(new SharedKernel.Result.Errors(envelope.ErrorList.ToArray()))
                       : GeneralErrors.Failure("Error while reading response");
            }*/

            if (envelope is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            if (envelope.ErrorList is not null)
            {
                return Result<TResponse>.Failure(new SharedKernel.Result.Errors(envelope.ErrorList.ToArray()));
            }

            /*if (envelope.Result is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }*/

            return envelope.Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return GeneralErrors.Failure("Error while reading response");
        }
    }

    public static async Task<Result> HandleResponseAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Envelope? envelope = await response.Content
                .ReadFromJsonAsync<Envelope>(cancellationToken);

            /*if (!response.IsSuccessStatusCode)
            {
                return envelope != null && envelope.ErrorList != null
                    ? Result.Failure(new SharedKernel.Result.Errors(envelope.ErrorList.ToArray()))
                    : GeneralErrors.Failure("Error while reading response");
            }*/

            if (envelope is null)
            {
                return GeneralErrors.Failure("Error while reading response");
            }

            if (envelope.ErrorList is not null)
            {
                return Result.Failure(new SharedKernel.Result.Errors(envelope.ErrorList.ToArray()));
            }

            return Result.Success();
        }
        catch
        {
            return GeneralErrors.Failure("Error while reading response");
        }
    }
}