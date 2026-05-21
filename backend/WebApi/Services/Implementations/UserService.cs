using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Contracts.User;
using WebApi.Constants;
using WebApi.Extensions.Mapping;

namespace WebApi.Services.Implementations;

/// <summary>
/// Service for managing user operations.
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<Role> roleManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<User> CreateAsync(CreateUserRequest request)
    {
        // Default: se IsAdmin è true, assegna ruolo Admin, altrimenti User
        var roleName = request.IsAdmin ? AuthConstants.Roles.Admin : AuthConstants.Roles.User;
        return await CreateAsync(request, roleName);
    }

    public async Task<User> CreateAsync(CreateUserRequest request, string roleName)
    {
        _logger.LogInformation($"Creating user: {request.Username} with role: {roleName}");

        // Verifica che il ruolo esista, altrimenti crealo
        await EnsureRoleExistsAsync(roleName);

        var user = request.MapToEntity();
        user.UserName = request.Username;
        user.Email = request.Email;

        // Crea l'utente con password
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to create user: {errors}");
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        // Assegna il ruolo
        var roleResult = await _userManager.AddToRoleAsync(user, roleName);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to assign role: {errors}");
            throw new InvalidOperationException($"Failed to assign role: {errors}");
        }

        // Sincronizza il campo legacy Admin con il ruolo
        user.Admin = (roleName == AuthConstants.Roles.Admin);
        await _userManager.UpdateAsync(user);

        _logger.LogInformation($"User created successfully: {user.Id} with role: {roleName}");
        return user;
    }

    public async Task<bool> UpdateAsync(string id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        user.UpdateFromRequest(request);

        // Se cambia lo stato Admin, aggiorna anche i ruoli
        if (request.IsAdmin.HasValue)
        {
            var isCurrentlyAdmin = await _userManager.IsInRoleAsync(user, AuthConstants.Roles.Admin);

            if (request.IsAdmin.Value && !isCurrentlyAdmin)
            {
                // Promuovi a Admin
                await _userManager.AddToRoleAsync(user, AuthConstants.Roles.Admin);
                await _userManager.RemoveFromRoleAsync(user, AuthConstants.Roles.User);
            }
            else if (!request.IsAdmin.Value && isCurrentlyAdmin)
            {
                // Degrada a User
                await _userManager.RemoveFromRoleAsync(user, AuthConstants.Roles.Admin);
                await _userManager.AddToRoleAsync(user, AuthConstants.Roles.User);
            }

            user.Admin = request.IsAdmin.Value;
        }

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return null;

        // Verifica la password
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded) return null;

        // Aggiorna LastLogin
        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation($"User logged in: {username}");
        return user;
    }

    public Task LogoutAsync()
    {
        // JWT tokens are stateless, logout is handled client-side
        return Task.CompletedTask;
    }

    public async Task<string?> ResetPasswordRequestAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return null;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        _logger.LogInformation($"Password reset token generated for user: {username}");

        return token;
    }

    public async Task<bool> ResetPasswordConfirmationAsync(string username, string token, string newPassword)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return false;

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            _logger.LogInformation($"Password reset successfully for user: {username}");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Password reset failed for user {username}: {errors}");
        }

        return result.Succeeded;
    }

    public async Task<bool> AssignRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Verifica che il ruolo esista
        await EnsureRoleExistsAsync(roleName);

        // Verifica se l'utente ha già il ruolo
        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            _logger.LogInformation($"User {userId} already has role {roleName}");
            return true;
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            // Sincronizza il campo Admin se necessario
            if (roleName == AuthConstants.Roles.Admin)
            {
                user.Admin = true;
                await _userManager.UpdateAsync(user);
            }

            _logger.LogInformation($"Assigned role {roleName} to user {userId}");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to assign role: {errors}");
        }

        return result.Succeeded;
    }

    public async Task<bool> RemoveRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            // Sincronizza il campo Admin se necessario
            if (roleName == AuthConstants.Roles.Admin)
            {
                user.Admin = false;
                await _userManager.UpdateAsync(user);
            }

            _logger.LogInformation($"Removed role {roleName} from user {userId}");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to remove role: {errors}");
        }

        return result.Succeeded;
    }

    public async Task<bool> AssignPermissionAsync(string userId, string permission)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Verifica se l'utente ha già questo permesso
        var existingClaims = await _userManager.GetClaimsAsync(user);
        if (existingClaims.Any(c => c.Type == AuthConstants.Claims.Permissions && c.Value == permission))
        {
            _logger.LogInformation($"User {userId} already has permission {permission}");
            return true;
        }

        var result = await _userManager.AddClaimAsync(user, 
            new System.Security.Claims.Claim(AuthConstants.Claims.Permissions, permission));

        if (result.Succeeded)
        {
            _logger.LogInformation($"Assigned permission {permission} to user {userId}");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to assign permission: {errors}");
        }

        return result.Succeeded;
    }

    public async Task<bool> RemovePermissionAsync(string userId, string permission)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var existingClaims = await _userManager.GetClaimsAsync(user);
        var claimToRemove = existingClaims
            .FirstOrDefault(c => c.Type == AuthConstants.Claims.Permissions && c.Value == permission);

        if (claimToRemove == null)
        {
            _logger.LogInformation($"User {userId} does not have permission {permission}");
            return true;
        }

        var result = await _userManager.RemoveClaimAsync(user, claimToRemove);

        if (result.Succeeded)
        {
            _logger.LogInformation($"Removed permission {permission} from user {userId}");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Failed to remove permission: {errors}");
        }

        return result.Succeeded;
    }

    public async Task<UserRolesAndPermissionsResponse?> GetUserRolesAndPermissionsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        
        var permissions = claims
            .Where(c => c.Type == AuthConstants.Claims.Permissions)
            .Select(c => c.Value)
            .ToList();

        var isAdmin = claims.Any(c => c.Type == AuthConstants.Claims.IsAdmin && c.Value == "true") ||
                     roles.Contains(AuthConstants.Roles.Admin);

        return new UserRolesAndPermissionsResponse
        {
            UserId = user.Id.ToString(),
            UserName = user.UserName ?? "",
            Roles = roles.ToList(),
            Permissions = permissions,
            IsAdmin = isAdmin
        };
    }

    /// <summary>
    /// Assicura che un ruolo esista nel database, altrimenti lo crea.
    /// </summary>
    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            _logger.LogInformation($"Creating role: {roleName}");
            var role = new Role { Name = roleName, NormalizedName = roleName.ToUpper() };
            await _roleManager.CreateAsync(role);
        }
    }
}