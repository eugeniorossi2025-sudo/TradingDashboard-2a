using Decisore.Middlewares;
using Decisore.Repository;
using Decisore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Connessione DB
builder.Services.AddScoped<DatabaseRepository>();

// Servizi statici
//builder.Services.AddSingleton<DeckStateService>();
builder.Services.AddSingleton<ProactiveEngineService>();

builder.Services.AddScoped<LoggingService>();

builder.Services.AddSingleton<AppStateService>();

builder.Services.AddHostedService<StartupInitializer>();

var app = builder.Build();

app.UseMiddleware<ApiLoggingMiddleware>();

app.MapControllers();

app.Run();

