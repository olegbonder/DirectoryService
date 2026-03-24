namespace FileService.Contracts.Dtos.MediaAssets.ListMultipartUpload;

public record MultipartUploadsResponse(IReadOnlyList<MultipartUploadDto> MultipartUploads);
