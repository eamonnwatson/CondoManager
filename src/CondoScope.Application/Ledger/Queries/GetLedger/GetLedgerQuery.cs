using FluentResults;
using MediatR;

namespace CondoScope.Application.Ledger.Queries.GetLedger;

public record GetLedgerQuery() : IRequest<Result<IReadOnlyList<LedgerDTO>>>;