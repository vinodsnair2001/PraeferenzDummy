using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Extensions;

namespace PraeferenzRoO.Persistence.Configurations;

internal sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("countries");

        builder.HasKey(x => x.Id).HasName("pk_countries");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.IsoCode2).HasMaxLength(2).IsRequired();
        builder.Property(x => x.IsoCode3).HasMaxLength(3).IsRequired();

        builder.HasIndex(x => x.IsoCode2)
            .HasDatabaseName("uix_countries_iso_code2")
            .IsUnique()
            .HasFilter("is_deleted = FALSE");

        builder.HasIndex(x => x.IsoCode3)
            .HasDatabaseName("uix_countries_iso_code3")
            .IsUnique()
            .HasFilter("is_deleted = FALSE");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_countries_tenant_id");

        builder.UseXminAsConcurrencyToken();

        builder.ConfigureAuditColumns();
    }
}
