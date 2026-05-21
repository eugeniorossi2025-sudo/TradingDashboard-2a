using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Contracts.Auth;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using WebApi.Constants;

namespace WebApi.Services.Implementations;

/// <summary>
/// Implementation of authentication operations using JWT tokens.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and generates a JWT token asynchronously.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        _logger.LogInformation($"Login attempt for username: {username}");
        
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            _logger.LogWarning($"Login failed: user not found - {username}");
            _logger.LogInformation($"Connection String DB: {_configuration.GetConnectionString("DefaultConnection")?.Substring(0, 50)}...");
            return null;
        }

        _logger.LogInformation($"User found: {user.UserName}, Email: {user.Email}, HasPassword: {!string.IsNullOrEmpty(user.PasswordHash)}");

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        
        _logger.LogInformation($"Password check result: Succeeded={result.Succeeded}, IsLockedOut={result.IsLockedOut}, IsNotAllowed={result.IsNotAllowed}, RequiresTwoFactor={result.RequiresTwoFactor}");
        
        if (!result.Succeeded)
        {
            _logger.LogWarning($"Login failed: invalid password - {username}");
            return null;
        }

        // Sincronizza il campo Admin con i ruoli di Identity
        var isInAdminRole = await _userManager.IsInRoleAsync(user, AuthConstants.Roles.Admin);

        if (user.Admin && !isInAdminRole)
        {
            // Se Admin=true ma non è nel ruolo, aggiungilo
            await _userManager.AddToRoleAsync(user, AuthConstants.Roles.Admin);
            _logger.LogInformation($"Added Admin role to user: {username}");
        }
        else if (!user.Admin && isInAdminRole)
        {
            // Se Admin=false ma è nel ruolo, rimuovilo
            await _userManager.RemoveFromRoleAsync(user, AuthConstants.Roles.Admin);
            _logger.LogInformation($"Removed Admin role from user: {username}");
        }

        // Aggiorna LastLogin
        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation($"User logged in successfully: {username}");
        return await GenerateJwtToken(user);
    }

    /// <summary>
    /// Logs out the current user asynchronously.
    /// </summary>
    public Task LogoutAsync()
    {
        // JWT tokens are stateless, logout is handled on the client side
        return Task.CompletedTask;
    }

    /// <summary>
    /// Generates a password reset token for the specified email.
    /// </summary>
    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning($"Password reset token requested for non-existent email: {email}");
            // Per sicurezza, non rivelare se l'email esiste o no
            return null;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // TODO: Invia email con il token
        // Per ora logga il token (SOLO PER SVILUPPO!)
        _logger.LogInformation($"Password reset token generated for {email}");
        
        return token;
    }

    /// <summary>
    /// Resets the password using a token and new password.
    /// </summary>
    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning($"Password reset attempted for non-existent email: {email}");
            return false;
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        
        if (result.Succeeded)
        {
            _logger.LogInformation($"Password reset successfully for user: {email}");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError($"Password reset failed for {email}: {errors}");
        }

        return result.Succeeded;
    }

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    private async Task<LoginResponse> GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];
        var jwtExpirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        // Ottieni i ruoli dell'utente
        var roles = await _userManager.GetRolesAsync(user);
        var isAdmin = roles.Contains(AuthConstants.Roles.Admin);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(AuthConstants.Claims.UserId, user.Id.ToString()),
            new Claim(AuthConstants.Claims.IsAdmin, isAdmin.ToString().ToLower()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Aggiungi ruoli come claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Ottieni i claims dal database
        var userClaims = await _userManager.GetClaimsAsync(user);
        
        // Aggiungi i claims dal database al token
        foreach (var userClaim in userClaims)
        {
            // Non duplicare i claims già aggiunti
            if (!claims.Any(c => c.Type == userClaim.Type && c.Value == userClaim.Value))
            {
                claims.Add(userClaim);
            }
        }

        // Se non ci sono claims di permessi nel database, aggiungi i permessi di default basati sul ruolo
        var hasPermissionClaims = userClaims.Any(c => c.Type == AuthConstants.Claims.Permissions);
        
        if (!hasPermissionClaims)
        {
            // Aggiungi permessi basati sul ruolo
            if (isAdmin)
            {
                claims.Add(new Claim(AuthConstants.Claims.Permissions, AuthConstants.AvailablePermissions.UserRead));
                claims.Add(new Claim(AuthConstants.Claims.Permissions, AuthConstants.AvailablePermissions.UserWrite));
                claims.Add(new Claim(AuthConstants.Claims.Permissions, AuthConstants.AvailablePermissions.UserDelete));
                claims.Add(new Claim(AuthConstants.Claims.Permissions, AuthConstants.AvailablePermissions.BotManage));
                claims.Add(new Claim(AuthConstants.Claims.Permissions, AuthConstants.AvailablePermissions.BotView));
                claims.Add(new Claim(AuthConstants.Claims.Permissions, AuthConstants.AvailablePermissions.ConfigManage));
            }
            else
            {
                claims.Add(new Claim(AuthConstants.Claims.Permissions, AuthConstants.AvailablePermissions.UserRead));
                claims.Add(new Claim(AuthConstants.Claims.Permissions, AuthConstants.AvailablePermissions.BotView));
            }
        }

        // Assicurati che il claim IsAdmin sia presente se l'utente ha il ruolo Admin
        if (isAdmin && !userClaims.Any(c => c.Type == AuthConstants.Claims.IsAdmin && c.Value == "true"))
        {
            // Il claim IsAdmin è già stato aggiunto sopra, ma verifica che sia "true"
            var isAdminClaim = claims.FirstOrDefault(c => c.Type == AuthConstants.Claims.IsAdmin);
            if (isAdminClaim == null || isAdminClaim.Value != "true")
            {
                claims.RemoveAll(c => c.Type == AuthConstants.Claims.IsAdmin);
                claims.Add(new Claim(AuthConstants.Claims.IsAdmin, "true"));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(jwtExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation($"JWT token generated for user: {user.UserName}, expires at: {expiration}");
        
        return new LoginResponse(tokenString, expiration);
    }
}