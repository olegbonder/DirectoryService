using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.DataBase.Configurations
{
    public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
    {
        public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
        {
            builder.ToTable("department_locations");

            builder.HasKey(dl => new { dl.DepartmentId, dl.LocationId }).HasName("pk_department_locations");

            builder.Property(dl => dl.LocationId).HasColumnName("location_id");
            builder.Property(dl => dl.DepartmentId).HasColumnName("department_id");

            builder.HasOne<Department>()
                .WithMany(d => d.DepartmentLocations)
                .HasForeignKey(dl => dl.LocationId)
                .HasConstraintName("fk_department_locations_location_id");

            builder.HasOne<Location>()
                .WithMany()
                .HasForeignKey(dl => dl.DepartmentId)
                .HasConstraintName("fk_department_locations_department_id");
        }
    }
}
