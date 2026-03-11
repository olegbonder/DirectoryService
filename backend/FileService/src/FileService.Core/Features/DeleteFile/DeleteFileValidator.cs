using Core.Validation;
using FluentValidation;
using SharedKernel.Result;

namespace FileService.Core.Features.DeleteFile;

public class DeleteFileValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileValidator()
    {
        RuleFor(f => f.FileId)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("FileId"))
            .NotEmpty()
            .WithError(GeneralErrors.PropertyIsEmpty("FileId", "FileId"));
    }
}