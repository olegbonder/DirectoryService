using Core.Validation;
using FileService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace FileService.Core.Features.CompleteMultipartUpload;

public class CompleteMultipartUploadValidator : AbstractValidator<CompleteMultipartUploadCommand>
{
    public CompleteMultipartUploadValidator()
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
        RuleFor(f => f.Request.PartETags.Count).GreaterThan(0)
            .WithError(MediaAssetErrors.PartETagsСountMustBePositive());
    }
}