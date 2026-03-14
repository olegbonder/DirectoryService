using Core.Validation;
using FileService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace FileService.Core.Features.AbortMultipartUpload;

public class AbortMultipartUploadValidator : AbstractValidator<AbortMultipartUploadCommand>
{
    public AbortMultipartUploadValidator()
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
    }
}