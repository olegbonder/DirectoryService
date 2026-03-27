using SharedKernel.Result;

namespace FileService.Domain.Shared;

public static class VideoProcessErrors
{
    public static Error VideoProcessMustHaveMoreOneStep()
    {
        return Error.Validation("video_process.has.not.steps", "VideoProcess must have more one step");
    }
}