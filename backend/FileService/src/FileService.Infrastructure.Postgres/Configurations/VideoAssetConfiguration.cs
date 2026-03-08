using FileService.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileService.Infrastructure.Postgres.Configurations;

public class VideoAssetConfiguration : IEntityTypeConfiguration<VideoAsset>
{
    public void Configure(EntityTypeBuilder<VideoAsset> builder)
    {
        builder.OwnsOne(v => v.HlsRootKey, rkb =>
        {
            rkb.ToJson("hls_root_key");
            rkb.Property(r => r.Bucket).HasColumnName("bucket").IsRequired();
            rkb.Property(r => r.Key).HasColumnName("key").IsRequired();
            rkb.Property(r => r.Prefix).HasColumnName("prefix");
            rkb.Property(r => r.Value).HasColumnName("value").IsRequired();
            rkb.Property(r => r.FullPath).HasColumnName("full_path").IsRequired();
        });
    }
}