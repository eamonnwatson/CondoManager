using CondoScope.Domain.Enums;
using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CondoScope.Application.Payments.Commands.CreatePayment;

public record CreatePaymentCommand(DateOnly PaymentDate, string UnitId, decimal Amount, PaymentMethod PaymentMethod, string? ReferenceNumber, string? Notes) : IRequest<Result<PaymentDto>>;
