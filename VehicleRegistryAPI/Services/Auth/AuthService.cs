using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VehicleRegistryAPI.Configurations;
using VehicleRegistryAPI.DTOS.Auth;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Tools.Exceptions;
using VehicleRegistryAPI.Tools.Security;

namespace VehicleRegistryAPI.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _jwtSettings = jwtSettings.Value; // 👈 Importante: usar Value
            _logger = logger;
        }

        public async Task<AuthResponseDto> Login(LoginDto dto)
        {
            _logger.LogInformation("Intento de inicio de sesión para email {Email}", dto.Email);

            try
            {
                // 1. Buscar usuario por email
                var user = await _userRepository.GetByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Inicio de sesión fallido: email {Email} no registrado", dto.Email);
                    throw new UnauthorizedException("Credenciales inválidas");
                }

                // 2. Verificar contraseña
                bool passwordValid = _passwordHasher.VerifyPassword(dto.Password, user.PasswordHash);
                if (!passwordValid)
                {
                    _logger.LogWarning("Inicio de sesión fallido: contraseña incorrecta para email {Email}", dto.Email);
                    throw new UnauthorizedException("Credenciales inválidas");
                }

                // 3. Generar token
                var token = _tokenService.GenerateAccessToken(user);
                var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

                _logger.LogInformation("Inicio de sesión exitoso para usuario {UserId} con email {Email}", user.Id, user.Email);

                return new AuthResponseDto
                {
                    AccessToken = token,
                    Expiration = expiration
                };
            }
            catch (Exception ex) when (ex is not UnauthorizedException)
            {
                _logger.LogError(ex, "Error inesperado durante el inicio de sesión para email {Email}", dto.Email);
                throw;
            }
        }
    }
}