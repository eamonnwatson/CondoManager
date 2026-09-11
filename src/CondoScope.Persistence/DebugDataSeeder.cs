using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using CondoScope.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence;

internal static class DebugDataSeeder
{
#if DEBUG
    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Units.AnyAsync(cancellationToken))
        {
            return;
        }

        const string createdBy = "debug-seed";

        var units = Enumerable.Range(1, 10)
            .Select(index => Unit.Create($"UNIT-{index:000}", $"{100 + index} Main Street", true, createdBy).Value)
            .ToList();

        var owners = Enumerable.Range(1, 10)
            .Select(index => Owner.Create(
                $"Owner {index}",
                Email.Create($"owner{index}@condoscope.local").Value,
                PhoneNumber.Create($"+1555000{index:000}").Value,
                createdBy).Value)
            .ToList();

        var ownershipStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));

        for (var index = 0; index < units.Count; index++)
        {
            var assignResult = units[index].AssignOwner(owners[index], ownershipStartDate.AddMonths(index), createdBy);
            if (assignResult.IsFailed)
            {
                throw new InvalidOperationException($"Failed to assign owner for seed unit {units[index].UnitNumber}.");
            }
        }

        var payments = new List<Payment>();
        for (var unitIndex = 0; unitIndex < units.Count; unitIndex++)
        {
            var unit = units[unitIndex];
            for (var paymentIndex = 0; paymentIndex < 3; paymentIndex++)
            {
                var paymentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(-(paymentIndex + 1)));
                var paymentResult = Payment.Create(
                    paymentDate,
                    unit,
                    350m + unitIndex * 10 + paymentIndex * 5,
                    paymentIndex % 2 == 0 ? PaymentMethod.ETransfer : PaymentMethod.Cheque,
                    $"PMT-{unitIndex + 1:00}-{paymentIndex + 1:00}",
                    "Debug seed payment",
                    createdBy);

                if (paymentResult.IsFailed)
                {
                    throw new InvalidOperationException($"Failed to create payment for seed unit {unit.UnitNumber}.");
                }

                payments.Add(paymentResult.Value);
            }
        }

        var feeCharges = new List<FeeCharge>();

        var condoFee = FeeCharge.Create(
            400m,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(15)),
            "Monthly condo contribution",
            ChargeCategory.CondoFee,
            ChargeScope.AllUnits,
            null,
            createdBy,
            units);

        if (condoFee.IsFailed)
        {
            throw new InvalidOperationException("Failed to create all-units condo fee seed data.");
        }

        feeCharges.Add(condoFee.Value);

        var reserveFee = FeeCharge.Create(
            125m,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(20)),
            "Reserve fund contribution",
            ChargeCategory.ReserveFee,
            ChargeScope.AllUnits,
            null,
            createdBy,
            units);

        if (reserveFee.IsFailed)
        {
            throw new InvalidOperationException("Failed to create all-units reserve fee seed data.");
        }

        feeCharges.Add(reserveFee.Value);

        for (var unitIndex = 0; unitIndex < units.Count; unitIndex++)
        {
            var unit = units[unitIndex];

            var elevatorRepair = FeeCharge.Create(
                90m + unitIndex,
                DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10 + unitIndex)),
                $"Elevator repair allocation for {unit.UnitNumber}",
                ChargeCategory.OtherFee,
                ChargeScope.SpecificUnit,
                unit,
                createdBy);

            if (elevatorRepair.IsFailed)
            {
                throw new InvalidOperationException($"Failed to create specific-unit fee for {unit.UnitNumber}.");
            }

            feeCharges.Add(elevatorRepair.Value);

            var balconyMaintenance = FeeCharge.Create(
                70m + unitIndex,
                DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(25 + unitIndex)),
                $"Balcony maintenance allocation for {unit.UnitNumber}",
                ChargeCategory.OtherFee,
                ChargeScope.SpecificUnit,
                unit,
                createdBy);

            if (balconyMaintenance.IsFailed)
            {
                throw new InvalidOperationException($"Failed to create second specific-unit fee for {unit.UnitNumber}.");
            }

            feeCharges.Add(balconyMaintenance.Value);
        }

        await dbContext.AddRangeAsync(units, cancellationToken);
        await dbContext.AddRangeAsync(owners, cancellationToken);
        await dbContext.AddRangeAsync(payments, cancellationToken);
        await dbContext.AddRangeAsync(feeCharges, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
#else
    public static Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
#endif
}
