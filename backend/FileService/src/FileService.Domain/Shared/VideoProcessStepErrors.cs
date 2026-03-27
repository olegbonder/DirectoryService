using FileService.Domain.MediaProcessing;
using SharedKernel.Result;

namespace FileService.Domain.Shared;

public static class VideoProcessStepErrors
{
    public static Error OrderMustBePositive()
    {
        return Error.Validation(
            "video_process_step.order.must.be.positive",
            "VideoProcess Step order must be positive");
    }

    public static Error OrderMustBeUnique()
    {
        return Error.Validation(
            "video_process_step.order.must.be.unique",
            "VideoProcess Step must have unique order");
    }

    public static Error NameIsEmpty()
    {
        return GeneralErrors.PropertyIsEmpty("video_process_step.name");
    }

    public static Result<VideoProcessProgress> ProgressOutOfRange(int min, int max)
    {
        return GeneralErrors.PropertyOutOfRange("video_process_step.progress", min, max);
    }

    public static Error NotFound()
    {
        return Error.NotFound("video_process_step.not.found", "Video process step not found");
    }

    public static Error InvalidStatus(string message)
    {
        return Error.Failure("step.invalid.status", message);
    }
}