using Core.Abstractions;
using FileService.Contracts.Dtos.MediaAssets.AbortMultipartUpload;

namespace FileService.Core.Features.AbortMultipartUpload;

public record AbortMultipartUploadCommand(AbortMultipartUploadRequest Request) : ICommand;