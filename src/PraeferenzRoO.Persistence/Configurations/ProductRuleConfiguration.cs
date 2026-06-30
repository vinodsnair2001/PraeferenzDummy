using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Extensions;

namespace PraeferenzRoO.Persistence.Configurations;

internal sealed class ProductRuleConfiguration : IEntityTypeConfiguration<ProductRule>
{
    public void Configure(EntityTypeBuilder<ProductRule> builder)
    {
        builder.ToTable("product_rules");

        builder.HasKey(x => x.Id).HasName("pk_product_rules");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.RuleName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.RuleCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Expression).HasColumnType("text");
        builder.Property(x => x.Condition).HasColumnType("text");
        builder.Property(x => x.ParametersJson).HasColumnType("text");

        builder.HasOne<TradeAgreement>()
            .WithMany()
            .HasForeignKey(x => x.TradeAgreementId)
            .HasConstraintName("fk_product_rules_trade_agreements")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .HasConstraintName("fk_product_rules_countries")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<HsCode>()
            .WithMany()
            .HasForeignKey(x => x.HsCodeId)
            .HasConstraintName("fk_product_rules_hs_codes")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.RuleCode)
            .HasDatabaseName("ix_product_rules_rule_code");

        builder.HasIndex(x => new { x.TradeAgreementId, x.HsCodeId })
            .HasDatabaseName("ix_product_rules_trade_agreement_hs_code");

        builder.HasIndex(x => x.TradeAgreementId)
            .HasDatabaseName("ix_product_rules_trade_agreement_id");

        builder.HasIndex(x => x.HsCodeId)
            .HasDatabaseName("ix_product_rules_hs_code_id");

        builder.HasIndex(x => x.CountryId)
            .HasDatabaseName("ix_product_rules_country_id");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_product_rules_tenant_id");

        builder.UseXminAsConcurrencyToken();

        builder.ConfigureAuditColumns();
    }
}
