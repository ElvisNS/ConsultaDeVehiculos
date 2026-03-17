using VehicleRegistryAPI.Entities;

namespace VehicleRegistryAPI.Services.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
    }
}
