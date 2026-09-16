using FluentResults;
using MediatR;

namespace CondoScope.Application.Statement.Queries;

public record GetStatementQuery(string UnitId) : IRequest<Result<StatementDto>>;
