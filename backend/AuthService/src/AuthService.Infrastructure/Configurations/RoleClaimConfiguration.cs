using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Configurations;

public class RoleClaimConfiguration: IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        builder.ToTable("role_claims");
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.RoleId).HasColumnName("role_id");
        builder.Property(r => r.ClaimType).HasColumnName("claim_type");
        builder.Property(r => r.ClaimValue).HasColumnName("claim_value");
    }
}