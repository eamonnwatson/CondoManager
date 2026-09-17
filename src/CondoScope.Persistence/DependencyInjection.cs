using CondoScope.Application.Common.Interfaces;
using CondoScope.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CondoScope.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string sqliteConnectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(sqliteConnectionString));

        services.AddScoped<IFeeChargeRepository, FeeChargeRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOwnersRepository, OwnerRepository>();
        services.AddScoped<IUnitsRepository, UnitRepository>();

        return services;
    }

    public static async Task ApplyPersistenceAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
        //await DebugDataSeeder.SeedAsync(dbContext, cancellationToken);
    }
}
