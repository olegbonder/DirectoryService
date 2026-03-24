using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets.GetMediaAsset;
using FileService.Domain.Shared;
using FluentValidation;

namespace FileService.Core.Features;

public class GetMediaAssetInfoValidator : AbstractValidator<GetMediaAssetInfoRequest>
{
    public GetMediaAssetInfoValidator()
    {
        RuleFor(f => f.MediaAssetId)
            .NotNull()
            .WithError(MediaAssetErrors.MediaAssetIdNotBeNull())
            .NotEmpty()
            .WithError(MediaAssetErrors.MediaAssetIdNotBeEmpty());
    }
}