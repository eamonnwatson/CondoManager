using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using FluentResults;
using FluentResults.Extensions;
using MediatR;

namespace CondoScope.Application.Statement.Queries;

internal class GetStatementQueryHandler(IUnitsRepository unitsRepository, IMapper mapper) : IRequestHandler<GetStatementQuery, Result<StatementDto>>
{
    public Task<Result<StatementDto>> Handle(GetStatementQuery request, CancellationToken cancellationToken) =>
        unitsRepository.GetByIdWithDetailsAsync(Ulid.Parse(request.UnitId), cancellationToken)
            .Map(mapper.Map<StatementDto>);

}
