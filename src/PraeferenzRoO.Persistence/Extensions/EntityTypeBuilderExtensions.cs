using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PraeferenzRoO.Domain.Common;

namespace PraeferenzRoO.Persistence.Extensions;

internal static class EntityTypeBuilderExtensions
{
    internal static EntityTypeBuilder<T> UseXminAsConcurrencyToken<T>(this EntityTypeBuilder<T> builder)
        where T : AggregateRoot
    {
        builder.Property<uint>("xmin").IsRowVersion();
        return builder;
    }

    internal static EntityTypeBuilder<T> ConfigureAuditColumns<T>(this EntityTypeBuilder<T> builder)
        where T : AuditableEntity
    {
        builder.Property(x => x.CreatedDate)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.ModifiedDate)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.DeletedDate)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(256);

        builder.Property(x => x.DeletedBy)
            .HasColumnName("deleted_by")
            .HasMaxLength(256);

        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.IPAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(x => x.Machine)
            .HasColumnName("machine")
            .HasMaxLength(256);

        return builder;
    }
}
