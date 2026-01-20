using DirectoryService.Domain;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.ToTable("positions");

            builder.HasKey(p => p.Id).HasName("pk_positions");

            builder.Property(d => d.Id)
                .HasConversion(
                    p => p.Value,
                    id => PositionId.Current(id))
                .HasColumnName("id");

            builder.OwnsOne(p => p.Name, pb =>
            {
                pb.Property(pn => pn.Value)
                    .IsRequired()
                    .HasMaxLength(LengthConstants.LENGTH_100)
                    .HasColumnName("name");
                pb.HasIndex(pn => pn.Value).IsUnique();
            });

            builder.ComplexProperty(p => p.Description, pb =>
            {
                pb.Property(pd => pd.Value)
                    .IsRequired(false)
                    .HasMaxLength(LengthConstants.LENGTH_1000)
                    .HasColumnName("description");
            });

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");

            builder.Property(p => p.DeletedAt)
                .HasColumnName("deleted_at");
        }
    }
}
