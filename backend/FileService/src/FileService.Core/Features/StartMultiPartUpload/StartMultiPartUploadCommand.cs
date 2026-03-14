using Core.Abstractions;
using FileService.Contracts.MediaAssets.StartMultiPartUpload;

namespace FileService.Core.Features.StartMultiPartUpload;

public record StartMultiPartUploadCommand(StartMultiPartUploadRequest Request) : ICommand;