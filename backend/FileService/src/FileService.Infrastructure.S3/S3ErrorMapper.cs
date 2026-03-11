using Amazon.S3;
using FileService.Domain;
using SharedKernel.Result;

namespace FileService.Infrastructure.S3;

public static class S3ErrorMapper
{
    public static Error ToError(Exception ex) => ex switch
    {
        AmazonS3Exception { ErrorCode: "NoSuchBucket" or "NoSuchKey" }
            => FileErrors.ObjectNotFound(),

        AmazonS3Exception { ErrorCode: "AccessDenied" }
            => FileErrors.Forbidden(),

        AmazonS3Exception { ErrorCode: "InvalidObjectState" }
        => FileErrors.Conflict(),

        HttpRequestException
            => FileErrors.NetworkIssue(),

        OperationCanceledException
            => FileErrors.OperationCanceled(),

        _ => FileErrors.InternalServerError(),
    };
}