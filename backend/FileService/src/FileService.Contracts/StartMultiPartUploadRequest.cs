namespace FileService.Contracts;

public record StartMultiPartUploadRequest
(
    string FileName,
    string AssetType,
    string ContentType,
    long Size,
    string Context,
    Guid ContextId
);