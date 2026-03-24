using Core.Validation;
using FileService.Contracts.Dtos.MediaAssets.GetMediaAssets;
using FileService.Domain.Shared;
using FluentValidation;

namespace FileService.Core.Features.GetMediaAssetInfo;

public class GetMediaAssetsInfoValidator : AbstractValidator<GetMediaAssetsRequest>
{
    public GetMediaAssetsInfoValidator()
    {
        RuleFor(f => f.MediaAssetIds)
                .NotNull()
                .WithError(MediaAssetErrors.MediaAssetIdsNotBeNull())
                .NotEmpty()
                .WithError(MediaAssetErrors.MediaAssetIdsNotBeEmpty())
                .Must(f => f != null && f.Distinct().Count() == f.Count())
                .WithError(MediaAssetErrors.MediaAssetIdsMustBeUnique());
    }
}