using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Extensions;

namespace PraeferenzRoO.Persistence.Configurations;

internal sealed class FinishedProductConfiguration : IEntityTypeConfiguration<FinishedProduct>
{
    public void Configure(EntityTypeBuilder<FinishedProduct> builder)
    {
        builder.ToTable("finished_products");

        builder.HasKey(x => x.Id).HasName("pk_finished_products");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.HsCodeValue).HasMaxLength(10).IsRequired();
        builder.Property(x => x.ExWorkPrice).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();

        builder.Navigation(x => x.Materials)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.HsCodeValue)
            .HasDatabaseName("ix_finished_products_hs_code_value");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_finished_products_tenant_id");

        builder.UseXminAsConcurrencyToken();

        builder.ConfigureAuditColumns();
    }
}
