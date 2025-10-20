using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.DataBase.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("locations");

            builder.HasKey(l => l.Id).HasName("pk_locations");

            builder.Property(l => l.Id)
                .HasConversion(
                    l => l.Value,
                    id => LocationId.Current(id))
                .HasColumnName("id");

            builder.OwnsOne(l => l.Name, lb =>
            {
                lb.Property(ln => ln.Value)
                    .IsRequired()
                    .HasMaxLength(LengthConstants.LENGTH_120)
                    .HasColumnName("name");
                lb.HasIndex(ln => ln.Value).IsUnique();
            });

            builder.OwnsOne(l => l.Address, lb =>
            {
                lb.ToJson("address");
                lb.Property(a => a.Country)
                    .IsRequired()
                    .HasColumnName("country");
                lb.Property(a => a.City)
                    .IsRequired()
                    .HasColumnName("city");
                lb.Property(a => a.Street)
                    .IsRequired()
                    .HasColumnName("street");
                lb.Property(a => a.HouseNumber)
                    .IsRequired()
                    .HasColumnName("houseNumber");
                lb.Property(a => a.FlatNumber)
                    .IsRequired(false)
                    .HasColumnName("flatNumber");
            });

            builder.OwnsOne(l => l.Timezone, lb =>
            {
                lb.Property(t => t.Value)
                .IsRequired()
                .HasColumnName("timezone");
            });

            builder.Property(l => l.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            builder.Property(l => l.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(l => l.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");
        }
    }
}
