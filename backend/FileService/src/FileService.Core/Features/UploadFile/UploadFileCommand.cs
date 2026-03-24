using Core.Abstractions;
using FileService.Contracts.Dtos.MediaAssets.UploadFile;

namespace FileService.Core.Features.UploadFile;

public record UploadFileCommand(UploadFileRequest Request) : ICommand;