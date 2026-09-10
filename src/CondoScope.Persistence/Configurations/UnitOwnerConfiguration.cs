using CondoScope.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoScope.Persistence.Configurations;

public class UnitOwnerConfiguration : IEntityTypeConfiguration<UnitOwner>
{
    public void Configure(EntityTypeBuilder<UnitOwner> builder)
    {
        builder.HasKey(uo => uo.Id);

        builder.Property(uo => uo.Id)
            .HasConversion(
                id => id.ToString(),
                id => Ulid.Parse(id));

        builder.Property(uo => uo.UnitId)
            .HasConversion(
                id => id.ToString(),
                id => Ulid.Parse(id));

        builder.Property(uo => uo.OwnerId)
            .HasConversion(
                id => id.ToString(),
                id => Ulid.Parse(id));

        builder.Property(uo => uo.EffectiveFrom)
            .IsRequired();

        builder.HasIndex(uo => new { uo.UnitId, uo.EffectiveFrom })
            .IsUnique();

        builder.HasIndex(uo => uo.OwnerId)
            .IsUnique();

        builder.HasOne(uo => uo.Owner)
            .WithOne(o => o.UnitOwner)
            .HasForeignKey<UnitOwner>(uo => uo.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
