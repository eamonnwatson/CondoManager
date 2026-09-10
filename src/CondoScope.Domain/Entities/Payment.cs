using CondoScope.Domain.Common;
using CondoScope.Domain.Enums;
using FluentResults;

namespace CondoScope.Domain.Entities;

public class Payment : BaseAuditableEntity
{
    private Payment()
    {
        Unit = null!;
    }

    private Payment(DateOnly paymentDate, Unit unit, decimal amount, PaymentMethod method, string? reference, string? notes,
                    DateTime createdAt, string createdBy) : base(Ulid.NewUlid(), createdAt, createdBy)
    {
        PaymentDate = paymentDate;
        UnitId = unit.Id;
        Unit = unit;
        Amount = amount;
        Method = method;
        Reference = reference;
        Notes = notes;
    }
    public DateOnly PaymentDate { get; set; }
    public Ulid UnitId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public Unit Unit { get; set; } = null!;

    public static Result<Payment> Create(DateOnly paymentDate, Unit unit, decimal amount, PaymentMethod method, string? reference, string? notes,
                                         string createdBy)
    {
        return new Payment(paymentDate, unit, amount, method, reference, notes, DateTime.UtcNow, createdBy);
    }
}
