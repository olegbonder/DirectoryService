using Core.Abstractions;
using FileService.Contracts.Dtos.MediaAssets.GetDownloadUrl;

namespace FileService.Core.Features.GetDownloadUrl;

public record GetDownloadUrlCommand(GetDownloadUrlRequest Request) : ICommand;