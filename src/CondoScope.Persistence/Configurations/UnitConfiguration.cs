using CondoScope.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoScope.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.ToString(),
                id => Ulid.Parse(id));

        builder.Property(u => u.UnitNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.Address)
            .HasMaxLength(300);

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.HasMany(u => u.UnitOwners)
            .WithOne(uo => uo.Unit)
            .HasForeignKey(uo => uo.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(u => u.UnitOwners)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(u => u.Payments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(u => u.FeeCharges)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
