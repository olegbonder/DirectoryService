using SharedKernel.Result;

namespace FileService.Core;

public interface IVideoProcessingScheduler
{
    Task<Result> ScheduleProcessingAsync(Guid videoAssetId, CancellationToken cancellationToken);
}
