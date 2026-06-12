using CM.TableNow.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.Run();

// Exposed for WebApplicationFactory<Program> integration tests.
public partial class Program;
