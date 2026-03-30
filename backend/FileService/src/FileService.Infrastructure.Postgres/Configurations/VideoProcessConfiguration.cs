using FileService.Domain.MediaProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations
{
    public class VideoProcessConfiguration : IEntityTypeConfiguration<VideoProcess>
    {
        public void Configure(EntityTypeBuilder<VideoProcess> builder)
        {
            builder.ToTable("video_processes");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Id).HasColumnName("id");
            builder.OwnsOne(v => v.RawKey, rkb =>
            {
                rkb.ToJson("raw_key");
                rkb.Property(r => r.Bucket).HasColumnName("bucket").IsRequired();
                rkb.Property(r => r.Key).HasColumnName("key").IsRequired();
                rkb.Property(r => r.Prefix).HasColumnName("prefix");
                rkb.Property(r => r.Value).HasColumnName("value").IsRequired();
                rkb.Property(r => r.FullPath).HasColumnName("full_path").IsRequired();
            });

            builder.OwnsOne(v => v.HlsKey, rkb =>
            {
                rkb.ToJson("hls_key");
                rkb.Property(r => r.Bucket).HasColumnName("bucket").IsRequired();
                rkb.Property(r => r.Key).HasColumnName("key").IsRequired();
                rkb.Property(r => r.Prefix).HasColumnName("prefix");
                rkb.Property(r => r.Value).HasColumnName("value").IsRequired();
                rkb.Property(r => r.FullPath).HasColumnName("full_path").IsRequired();
            });

            builder.OwnsOne(v => v.MetaData, mdb =>
            {
                mdb.ToJson("meta_data");
                mdb.Property(md => md.Duration).HasColumnName("duration").IsRequired();
                mdb.Property(md => md.Width).HasColumnName("width").IsRequired();
                mdb.Property(md => md.Height).HasColumnName("height").IsRequired();
            });

            builder.Property(v => v.Status).HasConversion<string>().HasColumnName("status");
            builder.Property(v => v.TotalProgress).HasColumnName("total_progress");
            builder.Property(v => v.ErrorMessage).HasColumnName("error_message");
            builder.Property(v => v.CreatedAt).HasColumnName("created_at");
            builder.Property(v => v.UpdatedAt).HasColumnName("updated_at");

            builder.HasMany(v => v.Steps)
                .WithOne()
                .HasForeignKey(s => s.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(v => new { v.Status }).HasDatabaseName("ix_video_processes_status");
            builder.HasIndex(v => new { v.Status, v.CreatedAt }).HasDatabaseName("ix_video_processes_status_created_at");
        }
    }
}