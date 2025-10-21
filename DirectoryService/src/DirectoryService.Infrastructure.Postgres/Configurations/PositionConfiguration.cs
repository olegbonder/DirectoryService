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

            builder.OwnsOne(p => p.Name, nb =>
            {
                nb.Property(d => d.Value)
                    .IsRequired()
                    .HasMaxLength(LengthConstants.LENGTH_100)
                    .HasColumnName("name");
                nb.HasIndex(p => p.Value).IsUnique();
            });

            builder.Property(p => p.Description)
                .HasConversion(
                    p => p.Value,
                    description => PositionDesription.Create(description).Value)
                .IsRequired(false)
                .HasMaxLength(LengthConstants.LENGTH_1000)
                .HasColumnName("description");

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");
        }
    }
}
