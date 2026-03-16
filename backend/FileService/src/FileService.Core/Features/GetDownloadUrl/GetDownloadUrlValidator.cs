using Core.Validation;
using FileService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace FileService.Core.Features.GetDownloadUrl;

public class GetDownloadUrlValidator : AbstractValidator<GetDownloadUrlCommand>
{
    public GetDownloadUrlValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.MediaAssetId)
            .NotNull()
            .WithError(MediaAssetErrors.MediaAssetIdNotBeNull())
            .NotEmpty()
            .WithError(MediaAssetErrors.MediaAssetIdNotBeEmpty());
    }
}