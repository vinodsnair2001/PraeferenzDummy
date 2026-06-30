using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Extensions;

namespace PraeferenzRoO.Persistence.Configurations;

internal sealed class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("materials");

        builder.HasKey(x => x.Id).HasName("pk_materials");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.HsCodeValue).HasMaxLength(10).IsRequired();
        builder.Property(x => x.OriginCountryCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.UnitCost).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();

        builder.HasIndex(x => x.HsCodeValue)
            .HasDatabaseName("ix_materials_hs_code_value");

        builder.HasIndex(x => x.OriginCountryCode)
            .HasDatabaseName("ix_materials_origin_country_code");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_materials_tenant_id");

        builder.UseXminAsConcurrencyToken();

        builder.ConfigureAuditColumns();
    }
}
