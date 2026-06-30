using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Extensions;

namespace PraeferenzRoO.Persistence.Configurations;

internal sealed class TradeAgreementConfiguration : IEntityTypeConfiguration<TradeAgreement>
{
    public void Configure(EntityTypeBuilder<TradeAgreement> builder)
    {
        builder.ToTable("trade_agreements");

        builder.HasKey(x => x.Id).HasName("pk_trade_agreements");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1024);

        builder.HasIndex(x => x.Code)
            .HasDatabaseName("ix_trade_agreements_code")
            .HasFilter("is_deleted = FALSE");

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .HasDatabaseName("uix_trade_agreements_tenant_code")
            .IsUnique()
            .HasFilter("is_deleted = FALSE");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_trade_agreements_tenant_id");

        builder.UseXminAsConcurrencyToken();

        builder.ConfigureAuditColumns();
    }
}
