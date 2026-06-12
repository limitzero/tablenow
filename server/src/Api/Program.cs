var builder = WebApplication.CreateBuilder(args);

// RegisterServices wired in task-03 (story-001) via builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.Run();

// Exposed for WebApplicationFactory<Program> integration tests.
public partial class Program;
