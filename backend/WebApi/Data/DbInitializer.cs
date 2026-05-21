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
}
