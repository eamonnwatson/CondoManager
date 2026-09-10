using CondoScope.Domain.Entities;
using CondoScope.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoScope.Persistence.Configurations;

public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.ToString(),
                id => Ulid.Parse(id));

        builder.Property(o => o.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.Email)
            .HasConversion(
                email => email == null ? null : email.Value,
                email => email == null ? null : Email.Create(email).Value)
            .HasMaxLength(200);

        builder.Property(o => o.Phone)
            .HasConversion(
                phone => phone == null ? null : phone.Value,
                phone => phone == null ? null : PhoneNumber.Create(phone).Value)
            .HasMaxLength(30);

        builder.HasOne(o => o.UnitOwner)
            .WithOne(uo => uo.Owner)
            .HasForeignKey<UnitOwner>(uo => uo.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
