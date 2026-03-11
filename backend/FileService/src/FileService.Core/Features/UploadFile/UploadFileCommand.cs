using Core.Abstractions;
using FileService.Contracts.MediaAssets.UploadFile;

namespace FileService.Core.Features.UploadFile;

public record UploadFileCommand(UploadFileRequest Request) : ICommand;