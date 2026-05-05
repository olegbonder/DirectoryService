using AuthService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("users");

        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(LengthConstants.LENGTH_100)
            .HasColumnName("first_name");

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(LengthConstants.LENGTH_100)
            .HasColumnName("last_name");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(u => u.UserName).HasColumnName("user_name");
        builder.Property(u => u.NormalizedUserName).HasColumnName("normalized_user_name");
        builder.Property(u => u.Email).HasColumnName("email");
        builder.Property(u => u.NormalizedEmail).HasColumnName("normalized_email");
        builder.Property(u => u.EmailConfirmed).HasColumnName("email_confirmed");
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash");
        builder.Property(u => u.SecurityStamp).HasColumnName("security_stamp");
        builder.Property(u => u.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        builder.Property(u => u.PhoneNumber).HasColumnName("phone_number");
        builder.Property(u => u.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
        builder.Property(u => u.LockoutEnabled).HasColumnName("lockout_enabled");
        builder.Property(u => u.LockoutEnd).HasColumnName("lockout_end");
        builder.Property(u => u.AccessFailedCount).HasColumnName("access_failed_count");
        builder.Property(u => u.TwoFactorEnabled).HasColumnName("two_factor_enabled");
    }
}