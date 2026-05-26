using System.ComponentModel.DataAnnotations;

namespace OrderSystem.DTOs;

public record RegisterRequest(
    [property: Required, MaxLength(100)] string Name,
    [property: Required, EmailAddress, MaxLength(200)] string Email,
    [property: Required, MinLength(8)] string Password
);

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password
);

public record AuthResponse(int UserId, string Name, string Email, string Role, string Token);
