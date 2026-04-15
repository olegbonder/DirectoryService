using FileService.Contracts.Dtos.MediaAssets;
using FileService.Contracts.Dtos.MediaAssets.ListMultipartUpload;
using FileService.Core.Models;
using FileService.Domain;
using SharedKernel.Result;

namespace FileService.Core.FilesStorage;

public interface IFileStorageProvider
{
    Task<Result> UploadFileAsync(
        StorageKey storageKey,
        Stream stream,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<string>> DownloadFileAsync(StorageKey storageKey, string tempPath,
        CancellationToken cancellationToken);

    Task<Result<string>> DeleteFileAsync(StorageKey storageKey, CancellationToken cancellationToken);

    Task<Result<string>> GenerateUploadUrlAsync(
        StorageKey storageKey,
        MediaData mediaData,
        CancellationToken cancellationToken);

    Task<Result<string>> GenerateUploadUrlAsync(StorageKey storageKey);

    Task<Result<string>> GenerateDownloadUrlAsync(StorageKey storageKey);

    Task<Result<IReadOnlyList<MediaUrl>>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken);

    Task<Result<string>> StartMultiPartUploadAsync(
        StorageKey storageKey,
        MediaData mediaData,
        CancellationToken cancellationToken);

    Task<Result<string>> GenerateChunkUploadUrlAsync(
        StorageKey storageKey,
        string uploadId,
        int partNumber,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ChunkUploadUrl>>> GenerateAllChunksUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result> CompleteMultiPartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartEtagDto> partETags,
        CancellationToken cancellationToken);

    Task<Result> AbortMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        CancellationToken cancellationToken);

    Task<Result<MultipartUploadsResponse>> ListMultipartUploadAsync(
        string bucketName,
        CancellationToken cancellationToken);
}
