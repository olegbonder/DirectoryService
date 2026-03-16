using Core.Abstractions;
using FileService.Contracts.MediaAssets.GetDownloadUrl;

namespace FileService.Core.Features.GetDownloadUrl;

public record GetDownloadUrlCommand(GetDownloadUrlRequest Request) : ICommand;