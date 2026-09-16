using System.Collections;
using System.Collections.Concurrent;

namespace CondoScope.Application.Common.Mapping;

internal class Mapper : IMapper
{
    private readonly ConcurrentDictionary<(Type Source, Type Destination), Func<object, object>> mappingFunctions = new();

    public TDestination Map<TDestination>(object source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return (TDestination)MapInternal(source.GetType(), typeof(TDestination), source)!;
    }

    public IEnumerable<TDestination> Map<TDestination>(IEnumerable source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = new List<TDestination>();
        foreach (var item in source)
        {
            if (item is null) continue;
            list.Add(Map<TDestination>(item));
        }

        return list;
    }

    public IMapper Register<TSource, TDestination>(Func<TSource, TDestination> mapFunction)
    {
        ArgumentNullException.ThrowIfNull(mapFunction);

        mappingFunctions[(typeof(TSource), typeof(TDestination))] =
            source => mapFunction((TSource)source)!;

        return this;
    }

    private object MapInternal(Type sourceType, Type destinationType, object source)
    {
        if (mappingFunctions.TryGetValue((sourceType, destinationType), out var mapFunction))
        {
            return mapFunction(source);
        }

        throw new InvalidOperationException($"No mapping function registered for {sourceType} to {destinationType}.");
    }

    public static Mapper Build(Action<Mapper> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var mapper = new Mapper();
        configure(mapper);
        return mapper;
    }
}