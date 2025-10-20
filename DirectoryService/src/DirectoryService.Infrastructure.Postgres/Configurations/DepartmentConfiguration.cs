using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.DataBase.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("departments");

            builder.HasKey(d => d.Id).HasName("pk_departments");

            builder.Property(d => d.Id)
                .HasConversion(d => d.Value, id => DepartmentId.Current(id))
                .HasColumnName("id");

            builder.OwnsOne(d => d.Name, db =>
            {
                db.Property(d => d.Value)
                    .IsRequired()
                    .HasMaxLength(LengthConstants.LENGTH_150)
                    .HasColumnName("name");
            });

            builder.OwnsOne(d => d.Identifier, db =>
            {
                db.Property(d => d.Value)
                    .IsRequired()
                    .HasMaxLength(LengthConstants.LENGTH_150)
                    .HasColumnName("identifier");
            });

            builder.OwnsOne(d => d.Path, db =>
            {
                db.Property(p => p.Value)
                    .IsRequired()
                    .HasColumnName("path");
            });

            builder.Property(d => d.Depth)
                .IsRequired()
                .HasColumnName("depth");

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            builder.Property(d => d.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(d => d.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");

            builder.HasMany(d => d.Children)
                .WithOne(d => d.Parent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_parent_id");

            builder.Property(d => d.ParentId)
                .HasColumnName("parent_id");
        }
    }
}
