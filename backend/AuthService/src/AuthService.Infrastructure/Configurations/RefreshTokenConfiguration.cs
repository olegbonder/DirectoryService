using AuthService.Domain;
using AuthService.Domain.Token;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(r => r.Id).HasName("pk_refresh_tokens");
        builder.Property(r => r.Id).HasColumnName("id");

        builder.OwnsOne(r => r.Token, rb =>
        {
            rb.Property(d => d.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.LENGTH_100)
                .HasColumnName("token");
        });

        builder.Property(r => r.UserId).HasColumnName("user_id");
        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .HasConstraintName("fk_refresh_tokens_user_id");

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(r => r.ExpiresAt)
            .IsRequired()
            .HasColumnName("expires_at");

        builder.Property(r => r.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(r => r.ReplacedByToken)
            .HasColumnName("replaced_by_token");
    }
}