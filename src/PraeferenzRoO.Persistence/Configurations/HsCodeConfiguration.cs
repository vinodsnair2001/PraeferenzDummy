using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Extensions;

namespace PraeferenzRoO.Persistence.Configurations;

internal sealed class HsCodeConfiguration : IEntityTypeConfiguration<HsCode>
{
    public void Configure(EntityTypeBuilder<HsCode> builder)
    {
        builder.ToTable("hs_codes");

        builder.HasKey(x => x.Id).HasName("pk_hs_codes");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512).IsRequired();
        builder.Property(x => x.ParentCode).HasMaxLength(10);

        builder.HasIndex(x => x.Code)
            .HasDatabaseName("ix_hs_codes_code")
            .HasFilter("is_deleted = FALSE");

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .HasDatabaseName("uix_hs_codes_tenant_code")
            .IsUnique()
            .HasFilter("is_deleted = FALSE");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_hs_codes_tenant_id");

        builder.UseXminAsConcurrencyToken();

        builder.ConfigureAuditColumns();
    }
}
