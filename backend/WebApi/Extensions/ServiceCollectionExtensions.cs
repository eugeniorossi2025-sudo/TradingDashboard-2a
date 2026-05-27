using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApi.Options;
using WebApi.Constants;
using WebApi.Data;
using WebApi.Services;
using WebApi.Services.Implementations;
using Entities;

namespace WebApi.Extensions;

/// <summary>
/// Extension methods for configuring application services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services, database contexts, authentication services, and validators.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DeciderOptions>(configuration.GetSection(DeciderOptions.SectionName));
        services.Configure<CollaudoOptions>(configuration.GetSection(CollaudoOptions.SectionName));
        services.AddHttpClient(nameof(Controllers.DeciderController));

        // Add DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Add ASP.NET Core Identity
        services.AddIdentity<User, Role>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Add FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Add JWT Authentication
        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            if (jwtKey != null)
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            }

            // Configura SignalR per JWT
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/dashboardHub"))
                    {
                        context.Token = accessToken;
                    }
                    
                    return Task.CompletedTask;
                }
            };
        });

        // Add Authorization Policies
        services.AddAuthorization(options =>
        {
            // Policy per Admin
            options.AddPolicy(AuthConstants.Policies.RequireAdmin, policy =>
                policy.RequireClaim(AuthConstants.Claims.IsAdmin, "true"));

            // Policy per User standard
            options.AddPolicy(AuthConstants.Policies.RequireUser, policy =>
                policy.RequireAuthenticatedUser());

            // Policy per Bot Operator (può gestire bot ma non utenti)
            options.AddPolicy(AuthConstants.Policies.RequireBotOperator, policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == AuthConstants.Claims.Permissions && 
                                               c.Value.Contains("bot.manage"))));

            // Policy combinata: Admin OR BotOperator
            options.AddPolicy(AuthConstants.Policies.RequireAdminOrBotOperator, policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == AuthConstants.Claims.IsAdmin && c.Value == "true") ||
                    context.User.HasClaim(c => c.Type == AuthConstants.Claims.Permissions && c.Value.Contains("bot.manage"))));
        });

        // Add Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPcCurrentStatusMirrorService, PcCurrentStatusMirrorService>();
        services.AddScoped<IValueService, ValueService>();
        services.AddScoped<ICommandService, CommandService>();
        services.AddScoped<IUserGridConfigurationService, UserGridConfigurationService>();
        services.AddScoped<IUserAccessTracker, UserAccessTracker>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();
        services.AddScoped<IMissionLifecycleService, MissionLifecycleService>();
        
        // Add Background Services
        services.AddHostedService<DashboardUpdateService>();

        return services;
    }
}