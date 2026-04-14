using System.Linq.Expressions;
using FileService.Domain.MediaProcessing;
using SharedKernel.Result;

namespace FileService.Core;

public interface IVideoProcessingRepository
{
    void Add(VideoProcess videoProcess);

    Task<Result<VideoProcess>> GetBy(
        Expression<Func<VideoProcess, bool>> predicate,
        CancellationToken cancellationToken);
}