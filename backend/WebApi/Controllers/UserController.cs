using Contracts.User;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Constants;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

/// <summary>
/// Controller for managing users and authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly RoleManager<Role> _roleManager;
    private readonly IRootOwnerGuard _rootOwnerGuard;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="roleManager">The role manager for retrieving roles from database.</param>
    public UserController(IUserService userService, RoleManager<Role> roleManager, IRootOwnerGuard rootOwnerGuard)
    {
        _userService = userService;
        _roleManager = roleManager;
        _rootOwnerGuard = rootOwnerGuard;
    }

    /// <summary>
    /// Gets all users (Admin only).
    /// </summary>
    /// <returns>A collection of all users.</returns>
    [HttpGet]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<User>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<User>>.SuccessResponse(users, "Users retrieved successfully"));
    }

    /// <summary>
    /// Gets a user by ID (Authenticated users can see their own profile, Admin can see all).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The user if found; otherwise, a not found result.</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = AuthConstants.Policies.RequireUser)]
    [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var currentUserId = User.FindFirst(AuthConstants.Claims.UserId)?.Value;
        var isAdmin = User.HasClaim(c => c.Type == AuthConstants.Claims.IsAdmin && c.Value == "true");

        // Gli utenti possono vedere solo il proprio profilo, gli admin vedono tutti
        if (currentUserId != id && !isAdmin)
        {
            return Forbid();
        }

        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        return Ok(ApiResponse<User>.SuccessResponse(user, "User retrieved successfully"));
    }

    /// <summary>
    /// Creates a new user (Admin only).
    /// </summary>
    /// <param name="request">The create user request.</param>
    /// <param name="roleName">Optional role name (Admin, User, BotOperator). Default is based on IsAdmin flag.</param>
    /// <returns>The created user.</returns>
    [HttpPost]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        [FromQuery] string? roleName = null)
    {
        try
        {
            User user;

            if (!string.IsNullOrEmpty(roleName))
            {
                // Crea con ruolo specifico
                user = await _userService.CreateAsync(request, roleName);
            }
            else
            {
                // Crea con ruolo default (basato su IsAdmin)
                user = await _userService.CreateAsync(request);
            }

            return CreatedAtAction(nameof(GetById), new { id = user.Id },
                ApiResponse<User>.SuccessResponse(user, "User created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Deletes a user (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>A no content result if successful; otherwise, a not found result.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string id)
    {
        if (int.TryParse(id, out var userId))
        {
            var blocked = await _rootOwnerGuard.BlockTargetMutationAsync(userId, "DELETE_USER", HttpContext);
            if (blocked != null) return blocked;
        }

        var result = await _userService.DeleteAsync(id);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        return Ok(ApiResponse<object>.SuccessResponse(new object(), "User deleted successfully"));
    }

    /// <summary>
    /// Gets all roles and permissions for a user (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The user's roles and permissions.</returns>
    [HttpGet("{id}/roles-and-permissions")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<UserRolesAndPermissionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRolesAndPermissions(string id)
    {
        var result = await _userService.GetUserRolesAndPermissionsAsync(id);
        if (result == null) return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        return Ok(ApiResponse<UserRolesAndPermissionsResponse>.SuccessResponse(result, "User roles and permissions retrieved successfully"));
    }

    /// <summary>
    /// Assigns a role to a user (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The role assignment request.</param>
    /// <returns>Success result if the role was assigned.</returns>
    [HttpPost("{id}/roles")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRole(string id, [FromBody] AssignRoleRequest request)
    {
        if (int.TryParse(id, out var userId))
        {
            var blocked = await _rootOwnerGuard.BlockTargetMutationAsync(userId, "ASSIGN_ROLE", HttpContext);
            if (blocked != null) return blocked;
        }

        var result = await _userService.AssignRoleAsync(id, request.RoleName);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("User not found or role assignment failed"));
        return Ok(ApiResponse<object>.SuccessResponse(new object(), $"Role '{request.RoleName}' assigned successfully"));
    }

    /// <summary>
    /// Removes a role from a user (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="roleName">The role name to remove.</param>
    /// <returns>Success result if the role was removed.</returns>
    [HttpDelete("{id}/roles/{roleName}")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(string id, string roleName)
    {
        if (int.TryParse(id, out var userId))
        {
            var blocked = await _rootOwnerGuard.BlockTargetMutationAsync(userId, "REMOVE_ROLE", HttpContext);
            if (blocked != null) return blocked;
        }

        var result = await _userService.RemoveRoleAsync(id, roleName);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("User not found or role removal failed"));
        return Ok(ApiResponse<object>.SuccessResponse(new object(), $"Role '{roleName}' removed successfully"));
    }

    /// <summary>
    /// Assigns a permission to a user (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The permission assignment request.</param>
    /// <returns>Success result if the permission was assigned.</returns>
    [HttpPost("{id}/permissions")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignPermission(string id, [FromBody] AssignPermissionRequest request)
    {
        if (int.TryParse(id, out var userId))
        {
            var blocked = await _rootOwnerGuard.BlockTargetMutationAsync(userId, "ASSIGN_PERMISSION", HttpContext);
            if (blocked != null) return blocked;
        }

        var result = await _userService.AssignPermissionAsync(id, request.Permission);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("User not found or permission assignment failed"));
        return Ok(ApiResponse<object>.SuccessResponse(new object(), $"Permission '{request.Permission}' assigned successfully"));
    }

    /// <summary>
    /// Removes a permission from a user (Admin only).
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="permission">The permission to remove.</param>
    /// <returns>Success result if the permission was removed.</returns>
    [HttpDelete("{id}/permissions/{permission}")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePermission(string id, string permission)
    {
        if (int.TryParse(id, out var userId))
        {
            var blocked = await _rootOwnerGuard.BlockTargetMutationAsync(userId, "REMOVE_PERMISSION", HttpContext);
            if (blocked != null) return blocked;
        }

        var result = await _userService.RemovePermissionAsync(id, permission);
        if (!result) return NotFound(ApiResponse<object>.ErrorResponse("User not found or permission removal failed"));
        return Ok(ApiResponse<object>.SuccessResponse(new object(), $"Permission '{permission}' removed successfully"));
    }

    /// <summary>
    /// Gets all available roles in the system from database (Admin only).
    /// </summary>
    /// <returns>A list of available roles.</returns>
    [HttpGet("available-roles")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableRoles()
    {
        // ✅ Recupera i ruoli dal database invece di usare mock
        var roles = await _roleManager.Roles
            .Select(r => r.Name)
            .Where(name => name != null)
            .ToListAsync();
        
        return Ok(ApiResponse<IEnumerable<string>>.SuccessResponse(roles!, "Available roles retrieved successfully"));
    }

    /// <summary>
    /// Gets all available permissions in the system from database (Admin only).
    /// </summary>
    /// <returns>A list of available permissions grouped by role.</returns>
    [HttpGet("available-permissions")]
    [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailablePermissions()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        
        var rolePermissions = new List<object>();
        
        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims
                .Where(c => c.Type == AuthConstants.Claims.Permissions)
                .Select(c => c.Value)
                .ToList();
            
            rolePermissions.Add(new
            {
                RoleName = role.Name,
                Permissions = permissions
            });
        }

        var allDefinedPermissions = AuthConstants.AvailablePermissions.GetAll();

        var result = new
        {
            DefinedPermissions = allDefinedPermissions,
            RolePermissions = rolePermissions
        };

        return Ok(ApiResponse<object>.SuccessResponse(result, "Available permissions retrieved successfully"));
    }
}