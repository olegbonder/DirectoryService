using System.Linq.Expressions;
using FileService.Core;
using FileService.Domain.MediaProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel.Result;

namespace FileService.Infrastructure.Postgres.Repositories
{
    public class VideoProcessingRepository : IVideoProcessingRepository
    {
        private readonly FileServiceDbContext _context;
        private readonly ILogger<VideoProcessingRepository> _logger;

        public VideoProcessingRepository(
            FileServiceDbContext context,
            ILogger<VideoProcessingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Add(VideoProcess videoProcess)
            => _context.VideoProcesses.Add(videoProcess);

        public async Task<Result<VideoProcess>> GetBy(
            Expression<Func<VideoProcess, bool>> predicate,
            CancellationToken cancellationToken)
        {
            var videoProcess = await _context.VideoProcesses
                .Include(v => v.Steps)
                .FirstOrDefaultAsync(predicate, cancellationToken);
            if (videoProcess == null)
                return GeneralErrors.NotFound("video_process", null);

            return videoProcess;
        }
    }
}