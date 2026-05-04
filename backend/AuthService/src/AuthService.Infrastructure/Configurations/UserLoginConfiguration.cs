using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Configurations;

public class UserLoginConfiguration: IEntityTypeConfiguration<IdentityUserLogin<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        builder.ToTable("user_logins");
        builder.Property(r => r.LoginProvider).HasColumnName("login_provider");
        builder.Property(r => r.ProviderKey).HasColumnName("provider_key");
        builder.Property(r => r.ProviderDisplayName).HasColumnName("provider_display_name");
        builder.Property(r => r.UserId).HasColumnName("user_id");
    }
}