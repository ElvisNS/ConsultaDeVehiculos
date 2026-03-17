using VehicleRegistryAPI.DTOS.Auth;

namespace VehicleRegistryAPI.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Login(LoginDto dto);
    }
}
