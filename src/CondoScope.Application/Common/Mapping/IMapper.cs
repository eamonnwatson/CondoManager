using System.Collections;

namespace CondoScope.Application.Common.Mapping;

internal interface IMapper
{
    TDestination Map<TDestination>(object source);
    IEnumerable<TDestination> Map<TDestination>(IEnumerable source);
    IMapper Register<TSource, TDestination>(Func<TSource, TDestination> mapFunction);
}
