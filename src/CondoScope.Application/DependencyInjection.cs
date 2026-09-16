using CondoScope.Application.Common.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CondoScope.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var currentAssembly = typeof(DependencyInjection).Assembly;
        services.AddSingleton<IMapper>(_ => Mapper.Build(m => RegisterAllMapModules(m, currentAssembly)));

        return services;
    }


    private static void RegisterAllMapModules(Mapper mapper, Assembly? assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var moduleTypes = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IMapModule).IsAssignableFrom(t));

        foreach (var type in moduleTypes)
        {
            var module = (IMapModule)Activator.CreateInstance(type)!;
            module.RegisterMaps(mapper);
        }
    }

}
