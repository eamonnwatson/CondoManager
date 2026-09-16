using CondoScope.Application.Common.Interfaces;
using CondoScope.Application.Common.Mapping;
using FluentResults;
using FluentResults.Extensions;
using MediatR;

namespace CondoScope.Application.FeeCharges.Queries.GetFeeCharges;

internal class GetFeeChargesQueryHandler(IFeeChargeRepository feeChargeRepository, IMapper mapper) : IRequestHandler<GetFeeChargesQuery, Result<IEnumerable<FeeChargeDto>>>
{
    public async Task<Result<IEnumerable<FeeChargeDto>>> Handle(GetFeeChargesQuery request, CancellationToken cancellationToken) =>
        await feeChargeRepository.GetAllAsync(cancellationToken)
            .Map(mapper.Map<FeeChargeDto>);
}
