using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Configurations
{
    public class DepartmentPostionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
    {
        public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
        {
            builder.ToTable("department_positions");

            builder.HasKey(dp => new { dp.DepartmentId, dp.PositionId })
                .HasName("pk_department_positions");

            builder.Property(dp => dp.PositionId)
                .HasConversion(
                    dp => dp.Value,
                    positionId => PositionId.Current(positionId))
                .IsRequired()
                .HasColumnName("position_id");

            builder.Property(dp => dp.DepartmentId)
                .HasConversion(
                    dp => dp.Value,
                    departmentId => DepartmentId.Current(departmentId))
                .IsRequired()
                .HasColumnName("department_id");

            builder.HasOne<Department>()
                .WithMany(dp => dp.DepartmentPositions)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("fk_department_positions_department_id");

            builder.HasOne<Position>()
                .WithMany()
                .HasForeignKey(dl => dl.PositionId)
                .HasConstraintName("fk_department_positions_position_id");
        }
    }
}
