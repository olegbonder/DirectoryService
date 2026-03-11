using FileService.Contracts;
using FileService.Domain;
using SharedKernel.Result;

namespace FileService.Core;

public interface IS3Provider
{
    Task<Result> UploadFileAsync(
        StorageKey storageKey,
        Stream stream,
        MediaData mediaData,
        CancellationToken cancellationToken);

    Task<Result<string>> DownloadFileAsync(StorageKey storageKey, string tempPath,
        CancellationToken cancellationToken);

    Task<Result<string>> DeleteFileAsync(StorageKey storageKey, CancellationToken cancellationToken);

    Task<Result<string>> GenerateUploadUrlAsync(
        StorageKey storageKey,
        MediaData mediaData,
        CancellationToken cancellationToken);

    Task<Result<string>> GenerateUploadUrlAsync(string bucketName, string key);

    Task<Result<string>> GenerateDownloadUrlAsync(StorageKey storageKey);

    Task<Result<string>> GenerateDownloadUrlAsync(string bucketName, string key);

    Task<Result<IReadOnlyList<string>>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken);

    Task<Result<string>> StartMultiPartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<string>>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string>> CompleteMultiPartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartEtagDto> partETags,
        CancellationToken cancellationToken);
}