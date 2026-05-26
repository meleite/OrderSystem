using System.ComponentModel.DataAnnotations;

namespace OrderSystem.DTOs;

public record RegisterRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(8)] string Password
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(int UserId, string Name, string Email, string Role, string Token);
