using Core.Abstractions;
using FileService.Contracts.MediaAssets.CompleteMultiPartUpload;

namespace FileService.Core.Features.CompleteMultipartUpload;

public record CompleteMultipartUploadCommand(CompleteMultiPartUploadRequest Request) : ICommand;