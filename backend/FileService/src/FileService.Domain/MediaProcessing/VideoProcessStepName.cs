using FileService.Domain.Shared;
using SharedKernel.Result;

namespace FileService.Domain.MediaProcessing;

public sealed record VideoProcessStepName
{
    private VideoProcessStepName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<VideoProcessStepName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return VideoProcessStepErrors.NameIsEmpty();
        }

        return new VideoProcessStepName(name);
    }
}