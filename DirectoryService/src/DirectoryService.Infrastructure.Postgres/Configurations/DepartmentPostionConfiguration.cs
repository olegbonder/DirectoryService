using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.DataBase.Configurations
{
    public class DepartmentPostionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
    {
        public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
        {
            builder.ToTable("department_positions");

            builder.HasKey(dl => new { dl.DepartmentId, dl.PositionId }).HasName("pk_department_positions");

            builder.Property(dl => dl.PositionId)
                .IsRequired()
                .HasColumnName("position_id");

            builder.Property(dl => dl.DepartmentId)
                .IsRequired()
                .HasColumnName("department_id");

            builder.HasOne<Department>()
                .WithMany(d => d.DepartmentPositions)
                .HasForeignKey(dl => dl.PositionId)
                .HasConstraintName("fk_department_positions_position_id");

            builder.HasOne<Position>()
                .WithMany()
                .HasForeignKey(dl => dl.DepartmentId)
                .HasConstraintName("fk_department_positions_department_id");
        }
    }
}
