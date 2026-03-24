using Core.Abstractions;
using FileService.Contracts.Dtos.MediaAssets.StartMultiPartUpload;

namespace FileService.Core.Features.StartMultiPartUpload;

public record StartMultiPartUploadCommand(StartMultiPartUploadRequest Request) : ICommand;