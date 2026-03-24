using Microsoft.AspNetCore.Http;

namespace FileService.Contracts.Dtos.MediaAssets.UploadFile;

public record UploadFileRequest(
    IFormFile File,
    string AssetType,
    string Context,
    Guid ContextId
);