using Core.Abstractions;

namespace FileService.Core.Features.DeleteFile;

public record DeleteFileCommand(Guid MediaAssetId) : ICommand;