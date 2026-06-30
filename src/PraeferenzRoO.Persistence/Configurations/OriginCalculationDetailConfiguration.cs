using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Extensions;

namespace PraeferenzRoO.Persistence.Configurations;

internal sealed class OriginCalculationDetailConfiguration : IEntityTypeConfiguration<OriginCalculationDetail>
{
    public void Configure(EntityTypeBuilder<OriginCalculationDetail> builder)
    {
        builder.ToTable("origin_calculation_details");

        builder.HasKey(x => x.Id).HasName("pk_origin_calculation_details");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.RuleName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Message).HasColumnType("text");
        builder.Property(x => x.EvidenceJson).HasColumnType("text");

        builder.HasOne<OriginCalculation>()
            .WithMany(oc => oc.Details)
            .HasForeignKey(x => x.OriginCalculationId)
            .HasConstraintName("fk_origin_calculation_details_origin_calculations")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OriginCalculationId)
            .HasDatabaseName("ix_origin_calculation_details_origin_calculation_id");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_origin_calculation_details_tenant_id");

        builder.ConfigureAuditColumns();
    }
}
