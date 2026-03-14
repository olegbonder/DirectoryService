using Core.Validation;
using FileService.Domain;
using FileService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace FileService.Core.Features.StartMultiPartUpload;

public class StartMultiPartUploadValidator : AbstractValidator<StartMultiPartUploadCommand>
{
    public StartMultiPartUploadValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.FileName).MustBeValueObject(FileName.Create);
        RuleFor(f => f.Request.ContentType).MustBeValueObject(ContentType.Create);
        RuleFor(f => f.Request.Size).GreaterThan(0).WithError(MediaAssetErrors.FileLength());
        RuleFor(f => f.Request.AssetType)
            .Must(t => Enum.TryParse<AssetType>(t, true, out _))
            .WithError(MediaAssetErrors.FailedAssetType());
        RuleFor(f => f.Request)
            .MustBeValueObject(r => MediaOwner.Create(r.Context, r.ContextId));
    }
}