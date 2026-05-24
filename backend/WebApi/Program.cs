using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using WebApi.Extensions;
using WebApi.Hubs;
using System.Reflection;
using WebApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Add services to the container
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .SelectMany(e => e.Value.Errors.Select(x => x.ErrorMessage))
                .ToList();

            var response = WebApi.Models.ApiResponse<object>.ErrorResponse(
                "Validation failed",
                errors
            );

            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddEndpointsApiExplorer();

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BotDashboard API",
        Version = "v1",
        Description = "API per la gestione del Bot Dashboard con telemetria, comandi e statistiche real-time"
    });

    // Abilita i commenti XML per la documentazione
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
    
    // Configurazione JWT in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add SignalR
builder.Services.AddSignalR();

// Add Application Services (DB, Auth, Services, Validators)
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Initialize database with roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("Database:EnsureCreated"))
        {
            var context = services.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();
        }

        await WebApi.Data.DbInitializer.Initialize(services, app.Configuration);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Abilita OpenAPI nativo di .NET 9
    app.MapOpenApi();
    
    // Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BotDashboard API v1");
        c.RoutePrefix = "swagger";
    });

    // Scalar API Reference
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("BotDashboard API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<DashboardHub>("/dashboardHub");

app.Run();