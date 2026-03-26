using FileService.Domain.MediaProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations;

public class VideoProcessStepConfiguration : IEntityTypeConfiguration<VideoProcessStep>
{
    public void Configure(EntityTypeBuilder<VideoProcessStep> builder)
    {
        builder.ToTable("video_process_steps");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.ProcessId).HasColumnName("process_id");
        
        builder.OwnsOne(s => s.Name, sb =>
        {
            sb.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("name");
        });

        builder.OwnsOne(s => s.Order, ob =>
        {
            ob.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("order");
        });

        builder.OwnsOne(s => s.Progress, pb =>
        {
            pb.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("progress");
        });
        builder.Property(s => s.Status).HasConversion<string>().HasColumnName("status");
        builder.Property(s => s.Error).HasColumnName("error");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        builder.Property(s => s.StartedAt).HasColumnName("started_at");
        builder.Property(s => s.CompletedAt).HasColumnName("completed_at");
    }
}