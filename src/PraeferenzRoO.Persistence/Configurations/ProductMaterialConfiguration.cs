using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Persistence.Extensions;

namespace PraeferenzRoO.Persistence.Configurations;

internal sealed class ProductMaterialConfiguration : IEntityTypeConfiguration<ProductMaterial>
{
    public void Configure(EntityTypeBuilder<ProductMaterial> builder)
    {
        builder.ToTable("product_materials");

        builder.HasKey(x => x.Id).HasName("pk_product_materials");
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalCost).HasPrecision(18, 4).IsRequired();

        builder.HasOne<FinishedProduct>()
            .WithMany(fp => fp.Materials)
            .HasForeignKey(x => x.FinishedProductId)
            .HasConstraintName("fk_product_materials_finished_products")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Material>()
            .WithMany()
            .HasForeignKey(x => x.MaterialId)
            .HasConstraintName("fk_product_materials_materials")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.FinishedProductId)
            .HasDatabaseName("ix_product_materials_finished_product_id");

        builder.HasIndex(x => x.MaterialId)
            .HasDatabaseName("ix_product_materials_material_id");

        builder.HasIndex(x => new { x.FinishedProductId, x.MaterialId })
            .HasDatabaseName("pix_product_materials_finished_product_material")
            .HasFilter("is_deleted = FALSE");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_product_materials_tenant_id");

        builder.ConfigureAuditColumns();
    }
}
