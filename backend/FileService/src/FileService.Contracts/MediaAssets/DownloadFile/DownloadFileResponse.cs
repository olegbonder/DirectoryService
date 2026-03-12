namespace FileService.Contracts.MediaAssets.DownloadFile;

public record DownloadFileResponse(
    string FileName,
    string ContentType,
    byte[] FileContents
);