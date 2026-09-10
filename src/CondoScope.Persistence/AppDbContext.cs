using CondoScope.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CondoScope.Persistence;

internal class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<UnitOwner> UnitOwners => Set<UnitOwner>();
    public DbSet<FeeCharge> FeeCharges => Set<FeeCharge>();
    public DbSet<FeeChargeUnit> FeeChargeUnits => Set<FeeChargeUnit>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
