using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using VehicleRegistryAPI.Configurations;
using VehicleRegistryAPI.DTOS.Auth;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Tools.Security;

namespace VehicleRegistryAPI.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly IUserRepository _userRepository;
        public AuthService(IOptions<JwtSettings> jwtSettings, IUserRepository userRepository, ITokenService tokenService)
        {
            _jwtSettings = new JwtSettings();
            _userRepository = userRepository;
            _tokenService = tokenService;
        }
        public async Task<AuthResponseDto> Login(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            // Generar token
            var token = _tokenService.GenerateAccessToken(user);

            return new AuthResponseDto
            {
                AccessToken = token,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
            };
        }
    }
}
