using CondoScope.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoScope.Persistence.Configurations;

public class FeeChargeConfiguration : IEntityTypeConfiguration<FeeCharge>
{
    public void Configure(EntityTypeBuilder<FeeCharge> builder)
    {
        builder.HasKey(fc => fc.Id);

        builder.Property(fc => fc.Id)
            .HasConversion(
                id => id.ToString(),
                id => Ulid.Parse(id));

        builder.Property(fc => fc.DueDate)
            .IsRequired();

        builder.Property(fc => fc.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(fc => fc.Category)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(fc => fc.Scope)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(fc => fc.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasMany(fc => fc.Units)
            .WithMany(u => u.FeeCharges)
            .UsingEntity<FeeChargeUnit>();

        builder.Navigation(fc => fc.Units)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
