using OrderSystem.Models;

namespace OrderSystem.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
