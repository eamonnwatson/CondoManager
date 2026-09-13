using CondoScope.Application.Ledger.Queries.GetLedger;
using CondoScope.Persistence;
using CondoScope.Web.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetLedgerQuery).Assembly));

var dbPath = builder.Configuration.GetConnectionString("CondoScopeDb") ?? Path.Combine(AppContext.BaseDirectory, "condoscope.db");

builder.Services.AddPersistence($"Data Source={dbPath}");
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


var app = builder.Build();

await app.Services.ApplyPersistenceAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
