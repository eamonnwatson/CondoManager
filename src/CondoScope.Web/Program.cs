using CondoScope.Application;
using CondoScope.Application.Ledger.Queries.GetLedger;
using CondoScope.Application.Statement.Pdf;
using CondoScope.Application.Statement.Queries;
using CondoScope.Persistence;
using CondoScope.Web.Components;
using MediatR;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetLedgerQuery).Assembly));

var configuredPath =
    builder.Configuration.GetValue<string>("DATABASE_PATH") ??
    builder.Configuration.GetConnectionString("CondoScopeDb") ??
    "condoscope.db";

var dbPath = Path.GetFullPath(configuredPath, AppContext.BaseDirectory);

builder.Services.AddApplication();
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

app.MapGet("/statement-pdf/{unitId}", async (string unitId, IMediator mediator, IStatementPdfGenerator pdfGenerator) =>
{
    var statementResult = await mediator.Send(new GetStatementQuery(unitId));
    if (statementResult.IsFailed)
        return Results.NotFound();

    var pdfBytes = pdfGenerator.Generate(statementResult.Value, DateOnly.FromDateTime(DateTime.Today));
    return Results.File(pdfBytes, "application/pdf");
});

app.Run();
