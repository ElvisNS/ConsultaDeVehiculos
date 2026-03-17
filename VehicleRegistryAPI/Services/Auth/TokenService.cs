using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VehicleRegistryAPI.Configurations;
using VehicleRegistryAPI.Entities;

namespace VehicleRegistryAPI.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;
        private static readonly JwtSecurityTokenHandler TokenHandler = new();

        public TokenService(IOptions<JwtSettings> jwtSettings, ILogger<TokenService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public string GenerateAccessToken(User user)
        {
            // Validar usuario
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(user.Email)) throw new ArgumentException("Email requerido", nameof(user));
            if (user.Id == 0) throw new ArgumentException("Id de usuario inválido", nameof(user)); // Ajusta según tipo de Id

            // Validar clave
            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            if (keyBytes.Length < 32)
            {
                _logger.LogWarning("La clave JWT tiene menos de 32 bytes ({KeyLength} bytes). Se recomienda usar una clave de al menos 256 bits.", keyBytes.Length);
            }

            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims básicos del usuario
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

            // Agregar roles (si existen)
            if (user.UserRoless != null && user.UserRoless.Any())
            {
                foreach (var userRole in user.UserRoless)
                {
                    if (userRole.Role != null && !string.IsNullOrWhiteSpace(userRole.Role.Name))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                    }
                }
            }
            else
            {
                _logger.LogDebug("El usuario {UdoserId} no tiene roles asignados.", user.Id);
            }

            // Configurar expiración, issuer y audience
            var issuer = string.IsNullOrEmpty(_jwtSettings.Issuer) ? "VehicleRegistryAPI" : _jwtSettings.Issuer;
            var audience = string.IsNullOrEmpty(_jwtSettings.Audience) ? "VehicleRegistryAPI" : _jwtSettings.Audience;
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes > 0 ? _jwtSettings.ExpirationMinutes : 15);

            // Crear token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            try
            {
                var token = TokenHandler.CreateToken(tokenDescriptor);
                return TokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar el token JWT para el usuario {UserId}", user.Id);
                throw new InvalidOperationException("No se pudo generar el token de acceso", ex);

            }
        }

    }
}
