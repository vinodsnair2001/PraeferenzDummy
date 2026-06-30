using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Extensions;

namespace PraeferenzRoO.Persistence.Configurations;

internal sealed class OriginCalculationConfiguration : IEntityTypeConfiguration<OriginCalculation>
{
    public void Configure(EntityTypeBuilder<OriginCalculation> builder)
    {
        builder.ToTable("origin_calculations");

        builder.HasKey(x => x.Id).HasName("pk_origin_calculations");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.DecisionSummary).HasColumnType("text");
        builder.Property(x => x.DecisionTreeJson).HasColumnType("text");

        builder.Navigation(x => x.Details)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne<FinishedProduct>()
            .WithMany()
            .HasForeignKey(x => x.FinishedProductId)
            .HasConstraintName("fk_origin_calculations_finished_products")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TradeAgreement>()
            .WithMany()
            .HasForeignKey(x => x.TradeAgreementId)
            .HasConstraintName("fk_origin_calculations_trade_agreements")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .HasConstraintName("fk_origin_calculations_countries")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FinishedProductId)
            .HasDatabaseName("ix_origin_calculations_finished_product_id");

        builder.HasIndex(x => x.TradeAgreementId)
            .HasDatabaseName("ix_origin_calculations_trade_agreement_id");

        builder.HasIndex(x => x.CountryId)
            .HasDatabaseName("ix_origin_calculations_country_id");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_origin_calculations_tenant_id");

        builder.UseXminAsConcurrencyToken();

        builder.ConfigureAuditColumns();
    }
}
