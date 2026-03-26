using FileService.Core;
using FileService.Domain.Assets;
using FileService.Domain.MediaProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileService.Infrastructure.Postgres;

public class FileServiceDbContext: DbContext, IReadDbContext
{
    public FileServiceDbContext(DbContextOptions<FileServiceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileServiceDbContext).Assembly);
    }

    private ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder => builder.AddConsole());
    }

    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public DbSet<PreviewAsset> PreviewAssets => Set<PreviewAsset>();

    public DbSet<VideoAsset> VideoAssets => Set<VideoAsset>();

    public DbSet<VideoProcess> VideoProcesses => Set<VideoProcess>();

    public DbSet<VideoProcessStep> VideoProcessSteps => Set<VideoProcessStep>();

    public IQueryable<MediaAsset> MediaAssetsQuery => MediaAssets.AsQueryable().AsNoTracking();
}