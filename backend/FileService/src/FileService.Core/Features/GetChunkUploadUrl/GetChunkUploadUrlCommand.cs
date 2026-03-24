using Core.Abstractions;
using FileService.Contracts.Dtos.MediaAssets.GetChunkUploadUrl;

namespace FileService.Core.Features.GetChunkUploadUrl;

public record GetChunkUploadUrlCommand(GetChunkUploadUrlRequest Request) : ICommand;