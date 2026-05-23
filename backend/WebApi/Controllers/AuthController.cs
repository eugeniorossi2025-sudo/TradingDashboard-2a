using Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

/// <summary>
/// Controller for managing authentication operations.
/// </summary>
namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[AllowAnonymous] // Allow anonymous access to all auth endpoints
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>A JWT token response if authentication is successful; otherwise, an unauthorized result.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request.Username, request.Password);
            return Ok(response);
        }
        catch
        {
            return Unauthorized("Invalid username or password");
        }
    }

    /// <summary>
    /// Test endpoint to verify API is running (no authentication required).
    /// </summary>
    /// <returns>A simple test response.</returns>
    [HttpGet("test")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Test()
    {
        return Ok(ApiResponse<object>.SuccessResponse(
            new { message = "Auth controller is working!", timestamp = DateTime.UtcNow },
            "Test successful"
        ));
    }

    /// <summary>
    /// Logs out the current authenticated user.
    /// </summary>
    /// <returns>An OK result.</returns>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok("Logged out successfully");
    }

    /// <summary>
    /// Step 1: Generates a password reset token for the specified email.
    /// Sends an email with the token (in production).
    /// </summary>
    /// <param name="request">The reset password request containing the email address.</param>
    /// <returns>Success message regardless of whether email exists (for security).</returns>
    [HttpPost("reset-password-request")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequest request)
    {
        var token = await _authService.GeneratePasswordResetTokenAsync(request.Email);
        
        if (token != null)
        {
            // TODO: In produzione, invia email con link tipo:
            // https://yourdomain.com/reset-password?email={email}&token={token}
            
            // Per sviluppo, ritorna il token nella risposta (RIMUOVI IN PRODUZIONE!)
            return Ok(ApiResponse<object>.SuccessResponse(
                new { Token = token, Email = request.Email }, 
                "Password reset token generated. Check your email."
            ));
        }
        
        // Per sicurezza, restituisci sempre successo anche se l'email non esiste
        // Così gli attaccanti non possono sapere quali email sono registrate
        return Ok(ApiResponse<object>.SuccessResponse(
            new object(), 
            "If the email exists, a password reset link has been sent."
        ));
    }

    /// <summary>
    /// Step 2: Confirms the password reset with the token and sets a new password.
    /// </summary>
    /// <param name="request">The reset password confirmation request containing email, token, and new password.</param>
    /// <returns>Success if password was reset; otherwise, an error.</returns>
    [HttpPost("reset-password-confirm")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordConfirmRequest request)
    {
        var success = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        
        if (!success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Invalid or expired token. Please request a new password reset."
            ));
        }
        
        return Ok(ApiResponse<object>.SuccessResponse(
            new object(), 
            "Password has been reset successfully. You can now login with your new password."
        ));
    }
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordConfirmRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}