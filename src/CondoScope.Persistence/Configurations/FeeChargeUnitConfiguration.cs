using CondoScope.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoScope.Persistence.Configurations;

public class FeeChargeUnitConfiguration : IEntityTypeConfiguration<FeeChargeUnit>
{
    public void Configure(EntityTypeBuilder<FeeChargeUnit> builder)
    {
        builder.HasKey(fcu => new { fcu.FeeChargeId, fcu.UnitId });

        builder.Property(fcu => fcu.FeeChargeId)
            .HasConversion(
                id => id.ToString(),
                id => Ulid.Parse(id));

        builder.Property(fcu => fcu.UnitId)
            .HasConversion(
                id => id.ToString(),
                id => Ulid.Parse(id));

        builder.HasOne(fcu => fcu.FeeCharge)
            .WithMany()
            .HasForeignKey(fcu => fcu.FeeChargeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fcu => fcu.Unit)
            .WithMany()
            .HasForeignKey(fcu => fcu.UnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}