using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Identity;
using WebApi.Constants;

namespace WebApi.Data;

/// <summary>
/// Initializes the database with default roles and admin user.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with roles and creates an admin user if it doesn't exist.
    /// </summary>
    public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting database initialization...");

        // 1. Create roles if they don't exist
        await CreateRolesAsync(roleManager, logger);

        // 2. Assign permissions to roles
        await AssignRolePermissionsAsync(roleManager, logger);

        // 3. Create admin user if it doesn't exist
        await CreateAdminUserAsync(userManager, configuration, logger);

        // 4. Ensure operational configuration keys exist in /pages/configuration
        await SeedOperationalConfigurationsAsync(serviceProvider, logger);

        logger.LogInformation("Database initialization completed successfully");
    }

    private static async Task CreateRolesAsync(RoleManager<Role> roleManager, ILogger logger)
    {
        string[] roleNames = { AuthConstants.Roles.Admin, AuthConstants.Roles.User, AuthConstants.Roles.BotOperator };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var role = new Role { Name = roleName };
                var result = await roleManager.CreateAsync(role);
                
                if (result.Succeeded)
                {
                    logger.LogInformation($"✅ Role '{roleName}' created successfully");
                }
                else
                {
                    logger.LogError($"❌ Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogInformation($"ℹ️ Role '{roleName}' already exists");
            }
        }
    }

    private static async Task AssignRolePermissionsAsync(RoleManager<Role> roleManager, ILogger logger)
    {
        // Permissions for Admin
        var adminRole = await roleManager.FindByNameAsync(AuthConstants.Roles.Admin);
        if (adminRole != null)
        {
            var adminPermissions = new[]
            {
                AuthConstants.AvailablePermissions.UserRead,
                AuthConstants.AvailablePermissions.UserWrite,
                AuthConstants.AvailablePermissions.UserDelete,
                AuthConstants.AvailablePermissions.BotManage,
                AuthConstants.AvailablePermissions.BotView,
                AuthConstants.AvailablePermissions.ConfigManage,
                AuthConstants.AvailablePermissions.DashboardView,
                AuthConstants.AvailablePermissions.DashboardManage,
                AuthConstants.AvailablePermissions.UsersView,
                AuthConstants.AvailablePermissions.UsersCreate,
                AuthConstants.AvailablePermissions.UsersEdit,
                AuthConstants.AvailablePermissions.UsersDelete,
                AuthConstants.AvailablePermissions.RolesView,
                AuthConstants.AvailablePermissions.RolesAssign,
                AuthConstants.AvailablePermissions.PermissionsView,
                AuthConstants.AvailablePermissions.PermissionsAssign,
                AuthConstants.AvailablePermissions.ConfigurationsView,
                AuthConstants.AvailablePermissions.ConfigurationsEdit,
                AuthConstants.AvailablePermissions.LogsView,
                AuthConstants.AvailablePermissions.DevicesView,
                AuthConstants.AvailablePermissions.DevicesCreate,
                AuthConstants.AvailablePermissions.DevicesEdit,
                AuthConstants.AvailablePermissions.DevicesDelete,
                AuthConstants.AvailablePermissions.CommandsExecute
            };

            await AssignPermissionsToRole(roleManager, adminRole, adminPermissions, logger);
        }

        // Permissions for User
        var userRole = await roleManager.FindByNameAsync(AuthConstants.Roles.User);
        if (userRole != null)
        {
            var userPermissions = new[]
            {
                AuthConstants.AvailablePermissions.UserRead,
                AuthConstants.AvailablePermissions.BotView,
                AuthConstants.AvailablePermissions.DashboardView,
                AuthConstants.AvailablePermissions.DevicesView,
                AuthConstants.AvailablePermissions.LogsView
            };

            await AssignPermissionsToRole(roleManager, userRole, userPermissions, logger);
        }

        // Permissions for BotOperator
        var botOperatorRole = await roleManager.FindByNameAsync(AuthConstants.Roles.BotOperator);
        if (botOperatorRole != null)
        {
            var botOperatorPermissions = new[]
            {
                AuthConstants.AvailablePermissions.UserRead,
                AuthConstants.AvailablePermissions.BotManage,
                AuthConstants.AvailablePermissions.BotView,
                AuthConstants.AvailablePermissions.DashboardView,
                AuthConstants.AvailablePermissions.DashboardManage,
                AuthConstants.AvailablePermissions.DevicesView,
                AuthConstants.AvailablePermissions.DevicesCreate,
                AuthConstants.AvailablePermissions.DevicesEdit,
                AuthConstants.AvailablePermissions.CommandsExecute,
                AuthConstants.AvailablePermissions.ConfigurationsView,
                AuthConstants.AvailablePermissions.LogsView
            };

            await AssignPermissionsToRole(roleManager, botOperatorRole, botOperatorPermissions, logger);
        }
    }

    private static async Task AssignPermissionsToRole(RoleManager<Role> roleManager, Role role, string[] permissions, ILogger logger)
    {
        var existingClaims = await roleManager.GetClaimsAsync(role);

        foreach (var permission in permissions)
        {
            var claimExists = existingClaims.Any(c => c.Type == AuthConstants.Claims.Permissions && c.Value == permission);
            
            if (!claimExists)
            {
                var result = await roleManager.AddClaimAsync(role, new Claim(AuthConstants.Claims.Permissions, permission));
                if (result.Succeeded)
                {
                    logger.LogInformation($"  ✅ Permission '{permission}' assigned to role '{role.Name}'");
                }
                else
                {
                    logger.LogError($"  ❌ Failed to assign permission '{permission}' to role '{role.Name}'");
                }
            }
        }
    }

    private static async Task CreateAdminUserAsync(UserManager<User> userManager, IConfiguration configuration, ILogger logger)
    {
        var adminUsername = configuration["Admin:Username"] ?? "admin";
        var adminPassword = configuration["Admin:Password"] ?? "Admin@123456";
        var adminEmail = configuration["Admin:Email"] ?? "admin@botdashboard.local";

        var adminUser = await userManager.FindByNameAsync(adminUsername);

        if (adminUser == null)
        {
            // Create new admin
            adminUser = new User
            {
                UserName = adminUsername,
                Email = adminEmail,
                Admin = true,
                Description = "System Administrator",
                LastLogin = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AuthConstants.Roles.Admin);
                logger.LogInformation($"✅ Admin user '{adminUsername}' created successfully");
                logger.LogWarning($"⚠️ Default admin credentials: {adminUsername} / {adminPassword}");
                logger.LogWarning("⚠️ CHANGE THIS PASSWORD IMMEDIATELY IN PRODUCTION!");
            }
            else
            {
                logger.LogError($"❌ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            logger.LogInformation($"ℹ️ Admin user '{adminUsername}' already exists");

            // Check and reset password if necessary (useful for migrated users)
            var hasPassword = await userManager.HasPasswordAsync(adminUser);
            if (!hasPassword || string.IsNullOrEmpty(adminUser.PasswordHash))
            {
                logger.LogWarning($"⚠️ Admin user '{adminUsername}' has no valid password, resetting...");
                
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                var result = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
                
                if (result.Succeeded)
                {
                    logger.LogInformation($"✅ Admin password reset to: {adminPassword}");
                }
            }

            // Ensure admin is in Admin role
            var isInAdminRole = await userManager.IsInRoleAsync(adminUser, AuthConstants.Roles.Admin);
            if (!isInAdminRole)
            {
                await userManager.AddToRoleAsync(adminUser, AuthConstants.Roles.Admin);
                logger.LogInformation($"✅ Added '{adminUsername}' to Admin role");
            }

            // Sync Admin field
            if (!adminUser.Admin)
            {
                adminUser.Admin = true;
                await userManager.UpdateAsync(adminUser);
                logger.LogInformation($"✅ Updated Admin field for '{adminUsername}'");
            }
        }
    }

    private static async Task SeedOperationalConfigurationsAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var defaults = new[]
        {
            new Configuration
            {
                Key = "SECURITY_FILTER_ENABLED",
                Description = "Security Filter per-bot: 1 attivo, 0 spento.",
                Value = "1",
                Pos = 900
            },
            new Configuration
            {
                Key = "SECURITY_FILTER_MAX_SHOE_HAND",
                Description = "Security Filter: applica il filtro entro questa mano dello shoe.",
                Value = "20",
                Pos = 901
            },
            new Configuration
            {
                Key = "SECURITY_FILTER_MIN_STREAK",
                Description = "Security Filter: streak minimo richiesto per aumentare il rischio.",
                Value = "5",
                Pos = 902
            },
            new Configuration
            {
                Key = "SECURITY_FILTER_MAX_AVG_SECONDS",
                Description = "Security Filter: media secondi mano sotto questa soglia aumenta il rischio.",
                Value = "23.5",
                Pos = 903
            },
            new Configuration
            {
                Key = "SECURITY_FILTER_VERY_FAST_SECONDS",
                Description = "Security Filter: media secondi mano molto veloce, ulteriore punto rischio.",
                Value = "21.0",
                Pos = 904
            },
            new Configuration
            {
                Key = "SECURITY_FILTER_DELTA_WINDOW",
                Description = "Security Filter: numero campioni usati per la media tempi per bot.",
                Value = "8",
                Pos = 905
            },
            new Configuration
            {
                Key = "SECURITY_FILTER_MIN_SCORE",
                Description = "Security Filter: score minimo su 4 per mettere in pausa solo quel bot.",
                Value = "3",
                Pos = 906
            },
            new Configuration
            {
                Key = "PLAYER_PACE_FILTER_ENABLED",
                Description = "Legacy alias Player Race 8 — usare PLAYER_RACE_8_ENABLED.",
                Value = "0",
                Pos = 907
            },
            new Configuration
            {
                Key = "PLAYER_RACE_5_ENABLED",
                Description = "Player Race 5: 1 attivo (5 PLAYER consecutivi), 0 spento.",
                Value = "0",
                Pos = 908
            },
            new Configuration
            {
                Key = "PLAYER_RACE_8_ENABLED",
                Description = "Legacy alias Player Race 8 — usare PLAYER_RACE_8_FILTER_ENABLED / PLAYER_RACE_8_AC3_ENABLED.",
                Value = "0",
                Pos = 909
            },
            new Configuration
            {
                Key = "PLAYER_RACE_5_FILTER_ENABLED",
                Description = "Player Race 5 filtro: 1 mostra avviso a 5 PLAYER consecutivi.",
                Value = "0",
                Pos = 910
            },
            new Configuration
            {
                Key = "PLAYER_RACE_5_AC3_ENABLED",
                Description = "Player Race 5 AC3: 1 genera AC3 a 5 PLAYER consecutivi.",
                Value = "0",
                Pos = 911
            },
            new Configuration
            {
                Key = "PLAYER_RACE_8_FILTER_ENABLED",
                Description = "Player Race 8 filtro: 1 mostra avviso a 8 PLAYER consecutivi.",
                Value = "0",
                Pos = 912
            },
            new Configuration
            {
                Key = "PLAYER_RACE_8_AC3_ENABLED",
                Description = "Player Race 8 AC3: 1 genera AC3 a 8 PLAYER consecutivi.",
                Value = "0",
                Pos = 913
            },
            new Configuration
            {
                Key = "SPOT_RESET_THRESHOLD_L5",
                Description = "Soglia L6 per bot: dopo N L5 persi nel ciclo SPOT, solo quel bot può passare a L6.",
                Value = "2",
                Pos = 914
            },
            new Configuration
            {
                Key = "SPOT_CYCLE_PB_HANDS",
                Description = "Mani PB per ciclo SPOT di ogni singolo bot; alla soglia si chiude il ciclo solo di quel bot.",
                Value = "600",
                Pos = 915
            },
            new Configuration
            {
                Key = "SPOT_L6_PER_BOT_ENABLED",
                Description = "SPOT L6 per bot: 1 attivo (L6 dopo N L5 perse nel ciclo SPOT del bot), 0 spento.",
                Value = "1",
                Pos = 916
            }
        };

        var added = 0;
        foreach (var item in defaults)
        {
            var exists = await context.Configurations.FindAsync(item.Key);
            if (exists != null)
            {
                continue;
            }

            context.Configurations.Add(item);
            added++;
        }

        if (added > 0)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} missing Security Filter configuration keys", added);
        }
    }
}
