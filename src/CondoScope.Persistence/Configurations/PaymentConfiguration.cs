using CondoScope.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoScope.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                id => id.ToString(),
                id => Ulid.Parse(id));

        builder.Property(p => p.UnitId)
            .HasConversion(
                id => id.ToString(),
                id => Ulid.Parse(id));

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Method)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.Reference)
            .HasMaxLength(100);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.HasOne(p => p.Unit)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
