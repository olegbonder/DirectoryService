namespace FileService.Contracts.MediaAssets.ListMultipartUpload;

public record MultipartUploadsResponse(IReadOnlyList<MultipartUploadDto> MultipartUploads);
