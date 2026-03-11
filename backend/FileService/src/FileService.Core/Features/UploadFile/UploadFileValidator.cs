using Core.Validation;
using FileService.Domain;
using FileService.Domain.Shared;
using FluentValidation;
using SharedKernel.Result;

namespace FileService.Core.Features.UploadFile;

public class UploadFileValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileValidator()
    {
        RuleFor(f => f.Request)
            .NotNull()
            .WithError(GeneralErrors.RequestIsNull());
        RuleFor(f => f.Request.File).NotNull().WithError(GeneralErrors.ValueIsRequired("file"));
        RuleFor(f => f.Request.File.FileName).MustBeValueObject(FileName.Create);
        RuleFor(f => f.Request.File.ContentType).MustBeValueObject(ContentType.Create);
        RuleFor(f => f.Request.File.Length).GreaterThan(0).WithError(MediaAssetErrors.FileLength());
        RuleFor(f => f.Request.AssetType)
            .Must(t => Enum.TryParse<AssetType>(t, true, out _))
            .WithError(MediaAssetErrors.FailedAssetType());
        RuleFor(f => f.Request)
            .MustBeValueObject(r => MediaOwner.Create(r.Context, r.ContextId));
    }
}