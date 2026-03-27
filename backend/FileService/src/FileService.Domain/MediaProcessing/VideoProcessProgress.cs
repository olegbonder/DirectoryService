using FileService.Domain.Shared;
using SharedKernel.Result;

namespace FileService.Domain.MediaProcessing;

public sealed record VideoProcessProgress
{
    public const int MIN_LENGTH = 0;
    public const int MAX_LENGTH = 100;
    private VideoProcessProgress(double value)
    {
        Value = value;
    }

    public double Value { get; }

    public static Result<VideoProcessProgress> Create(double percent)
    {
        if (percent < MIN_LENGTH || percent > MAX_LENGTH)
        {
            return VideoProcessStepErrors.ProgressOutOfRange(MIN_LENGTH, MAX_LENGTH);
        }

        return new VideoProcessProgress(percent);
    }
}