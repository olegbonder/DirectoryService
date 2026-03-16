using Core.Validation;
using FileService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace FileService.Core.Features.GetChunkUploadUrl;

public class GetChunkUploadUrlValidator : AbstractValidator<GetChunkUploadUrlCommand>
{
    public GetChunkUploadUrlValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.MediaAssetId)
            .NotNull()
            .WithError(MediaAssetErrors.MediaAssetIdNotBeNull())
            .NotEmpty()
            .WithError(MediaAssetErrors.MediaAssetIdNotBeEmpty());
        RuleFor(f => f.Request.UploadId)
            .NotNull()
            .WithError(MediaAssetErrors.UploadIdNotBeNull())
            .NotEmpty()
            .WithError(MediaAssetErrors.UploadIdNotBeEmpty());
        RuleFor(f => f.Request.PartNumber).GreaterThan(0)
            .WithError(MediaAssetErrors.PartNumberMustBePositive());
    }
}