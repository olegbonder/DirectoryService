using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Configurations;

public class UserTokenConfiguration: IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder.ToTable("user_tokens");
        builder.Property(r => r.UserId).HasColumnName("user_id");
        builder.Property(r => r.LoginProvider).HasColumnName("login_provider");
        builder.Property(r => r.Name).HasColumnName("name");
        builder.Property(r => r.Value).HasColumnName("value");
    }
}