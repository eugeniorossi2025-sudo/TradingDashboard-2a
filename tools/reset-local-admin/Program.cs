using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApi.Data;

static string FindWebApiDir()
{
    for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
    {
        var candidate = Path.Combine(dir.FullName, "backend", "WebApi");
        if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            return candidate;
    }
    throw new DirectoryNotFoundException("backend/WebApi not found from " + AppContext.BaseDirectory);
}

static (bool prod, int? userId, string? username, string? password) ParseArgs(string[] args)
{
    var prod = args.Contains("--prod", StringComparer.OrdinalIgnoreCase);
    int? userId = null;
    string? username = null;
    string? password = null;
    for (var i = 0; i < args.Length; i++)
    {
        if (args[i].Equals("--user-id", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length
            && int.TryParse(args[i + 1], out var id))
        {
            userId = id;
            i++;
        }
        else if (args[i].Equals("--username", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
        {
            username = args[i + 1];
            i++;
        }
        else if (args[i].Equals("--password", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
        {
            password = args[i + 1];
            i++;
        }
    }
    return (prod, userId, username, password);
}

var (useProdConfig, targetUserId, targetUsername, newPassword) = ParseArgs(args);

var webApiDir = FindWebApiDir();
var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(webApiDir)
    .AddJsonFile("appsettings.json", optional: false);

if (useProdConfig)
{
    const string prodPath = @"C:\inetpub\wwwroot\shared\appsettings.Production.json";
    if (!File.Exists(prodPath))
    {
        Console.Error.WriteLine($"Production config not found: {prodPath}");
        return 10;
    }
    configurationBuilder.AddJsonFile(prodPath, optional: false);
}
else
{
    configurationBuilder.AddJsonFile("appsettings.LocalProdLike.json", optional: true);
}

var configuration = configurationBuilder
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
services.AddDbContext<AppDbContext>(o => o.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
services.AddIdentity<User, IdentityRole<int>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

await using var provider = services.BuildServiceProvider();
var userManager = provider.GetRequiredService<UserManager<User>>();

User? user = null;
if (targetUserId.HasValue)
{
    user = await userManager.FindByIdAsync(targetUserId.Value.ToString());
    if (user is null)
    {
        Console.Error.WriteLine($"User not found: Id={targetUserId}");
        return 1;
    }
}
else
{
    var username = targetUsername ?? configuration["Admin:Username"] ?? "admin";
    user = await userManager.FindByNameAsync(username);
    if (user is null)
    {
        Console.Error.WriteLine($"User not found: {username}");
        return 1;
    }
}

if (targetUserId == 13 && !user.IsRootOwner)
{
    Console.Error.WriteLine("Refusing: UserId 13 is not marked IsRootOwner.");
    return 3;
}

var password = newPassword ?? configuration["Admin:Password"] ?? "Admin@123456";

var token = await userManager.GeneratePasswordResetTokenAsync(user);
var result = await userManager.ResetPasswordAsync(user, token, password);
if (!result.Succeeded)
{
    Console.Error.WriteLine(string.Join("; ", result.Errors.Select(e => e.Description)));
    return 2;
}

// Clear lockout without touching protected root-owner columns via direct SQL
if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
{
    await userManager.SetLockoutEndDateAsync(user, null);
    await userManager.ResetAccessFailedCountAsync(user);
}

Console.WriteLine($"OK password reset for UserName={user.UserName} Id={user.Id}");
return 0;
