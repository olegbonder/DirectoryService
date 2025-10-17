using DirectoryService.Domain;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.DataBase.Configurations
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> buipder)
        {
            buipder.ToTable("positions");

            buipder.HasKey(p => p.Id).HasName("pk_positions");

            buipder.Property(p => p.Id).HasColumnName("id");

            buipder.Property(p => p.Name)
                .HasConversion(
                    p => p.Value,
                    name => PositionName.Create(name).Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH_100)
                .HasColumnName("name");

            buipder.HasIndex(p => p.Name).IsUnique();

            buipder.Property(p => p.Description)
                .HasConversion(
                    p => p.Value,
                    description => PositionDesription.Create(description).Value)
                .IsRequired(false)
                .HasMaxLength(LengthConstants.LENGTH_1000)
                .HasColumnName("description");

            buipder.Property(p => p.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            buipder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            buipder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");
        }
    }
}
