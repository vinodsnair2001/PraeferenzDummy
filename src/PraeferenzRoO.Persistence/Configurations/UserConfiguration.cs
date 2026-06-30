using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Extensions;

namespace PraeferenzRoO.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id).HasName("pk_users");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Username).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();

        // SECURITY: password_hash and refresh_token_hash are sensitive columns.
        // Never include these in Dapper SELECT projections or API response DTOs.
        builder.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.RefreshTokenHash).HasMaxLength(256);

        builder.HasIndex(x => x.Email)
            .HasDatabaseName("ix_users_email")
            .HasFilter("is_deleted = FALSE");

        builder.HasIndex(x => new { x.TenantId, x.Email })
            .HasDatabaseName("uix_users_tenant_email")
            .IsUnique()
            .HasFilter("is_deleted = FALSE");

        builder.HasIndex(x => new { x.TenantId, x.Username })
            .HasDatabaseName("uix_users_tenant_username")
            .IsUnique()
            .HasFilter("is_deleted = FALSE");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_users_tenant_id");

        builder.UseXminAsConcurrencyToken();

        builder.ConfigureAuditColumns();
    }
}
