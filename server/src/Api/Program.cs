using CM.TableNow.Api.Endpoints;
using CM.TableNow.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

await app.SeedDatabaseAsync();

var api = app.MapGroup("/api");
api.MapRestaurantEndpoints();

app.Run();

// Exposed for WebApplicationFactory<Program> integration tests.
public partial class Program;
