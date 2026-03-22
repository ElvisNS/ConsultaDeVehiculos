using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VehicleRegistryAPI.Configurations;
using VehicleRegistryAPI.DTOS.Auth;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Services.Auth;
using VehicleRegistryAPI.Tools.Exceptions;
using VehicleRegistryAPI.Tools.Security;

namespace VehicleRegistryAPI.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IOptions<JwtSettings>> _mockJwtSettings;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;
        private readonly JwtSettings _jwtSettings;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTokenService = new Mock<ITokenService>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _jwtSettings = new JwtSettings
            {
                Key = "test-secret-key-minimum-32-characters",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationMinutes = 60
            };

            _mockJwtSettings.Setup(x => x.Value).Returns(_jwtSettings);

            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockTokenService.Object,
                _mockPasswordHasher.Object,
                _mockJwtSettings.Object,
                _mockLogger.Object);
        }

        #region Login Tests

        [Fact]
        public async Task Login_ConCredencialesValidas_RetornaAuthResponseDto()
        {
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = loginDto.Email,
                UserName = "testuser",
                PasswordHash = "hashedPassword",
                IsActive = true
            };

            var expectedToken = "jwt-token-example";
            var expectedExpiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.VerifyPassword(loginDto.Password, user.PasswordHash)).Returns(true);
            _mockTokenService.Setup(t => t.GenerateAccessToken(user)).Returns(expectedToken);

            var result = await _authService.Login(loginDto);

            Assert.NotNull(result);
            Assert.Equal(expectedToken, result.AccessToken);
            Assert.Equal(_jwtSettings.ExpirationMinutes, (result.Expiration - DateTime.UtcNow).TotalMinutes, precision: 1);
        }

        [Fact]
        public async Task Login_ConEmailNoRegistrado_LanzaUnauthorizedException()
        {
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync((User?)null);

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.Login(loginDto));

            Assert.Equal("Credenciales inválidas", exception.Message);
            _mockPasswordHasher.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockTokenService.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_ConContrasenaIncorrecta_LanzaUnauthorizedException()
        {
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            var user = new User
            {
                Id = 1,
                Email = loginDto.Email,
                UserName = "testuser",
                PasswordHash = "hashedPassword",
                IsActive = true
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.VerifyPassword(loginDto.Password, user.PasswordHash)).Returns(false);

            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.Login(loginDto));

            Assert.Equal("Credenciales inválidas", exception.Message);
            _mockTokenService.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Login_VerificaLlamadaGetByEmailAsync()
        {
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = loginDto.Email,
                UserName = "testuser",
                PasswordHash = "hashedPassword"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockTokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("token");

            await _authService.Login(loginDto);

            _mockUserRepository.Verify(r => r.GetByEmailAsync(loginDto.Email), Times.Once);
        }

        [Fact]
        public async Task Login_VerificaLlamadaVerifyPassword()
        {
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = loginDto.Email,
                UserName = "testuser",
                PasswordHash = "hashedPassword"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.VerifyPassword(loginDto.Password, user.PasswordHash)).Returns(true);
            _mockTokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("token");

            await _authService.Login(loginDto);

            _mockPasswordHasher.Verify(p => p.VerifyPassword(loginDto.Password, user.PasswordHash), Times.Once);
        }

        [Fact]
        public async Task Login_VerificaLlamadaGenerateAccessToken()
        {
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = loginDto.Email,
                UserName = "testuser",
                PasswordHash = "hashedPassword"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockTokenService.Setup(t => t.GenerateAccessToken(user)).Returns("jwt-token");

            await _authService.Login(loginDto);

            _mockTokenService.Verify(t => t.GenerateAccessToken(user), Times.Once);
        }

        [Fact]
        public async Task Login_VerificaExpirationCorrecto()
        {
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = loginDto.Email,
                UserName = "testuser",
                PasswordHash = "hashedPassword"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockTokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("token");

            var beforeLogin = DateTime.UtcNow;
            var result = await _authService.Login(loginDto);
            var afterLogin = DateTime.UtcNow;

            var expectedExpirationMin = _jwtSettings.ExpirationMinutes;
            var actualExpirationMin = (result.Expiration - beforeLogin).TotalMinutes;

            Assert.True(actualExpirationMin >= expectedExpirationMin - 1 && actualExpirationMin <= expectedExpirationMin + 1,
                $"Expiration should be approximately {expectedExpirationMin} minutes from now, but was {actualExpirationMin}");
        }

        [Fact]
        public async Task Login_WhenUserExistsButIsInactive_ThrowsUnauthorizedExceptionAndLogsWarning()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "inactive@example.com",
                Password = "anyPassword"
            };

            var user = new User
            {
                Id = 1,
                Email = loginDto.Email,
                PasswordHash = "hashedPassword",
                IsActive = false // Inactivo
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            // No configuramos PasswordHasher porque nunca se llegará a verificar la contraseña

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _authService.Login(loginDto));

            Assert.Equal("Credenciales inválidas", exception.Message);

            // Verificar que NO se llamó a PasswordHasher ni a TokenService
            _mockPasswordHasher.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockTokenService.Verify(t => t.GenerateAccessToken(It.IsAny<User>()), Times.Never);

            // Verificar que se registró la advertencia correspondiente
            _mockLogger.Verify(
                 x => x.Log(
                     LogLevel.Warning,
                     It.IsAny<EventId>(),
                     It.Is<It.IsAnyType>((v, t) =>
                         v.ToString().Contains("Inicio de sesión fallido para email") &&
                         v.ToString().Contains(loginDto.Email)),
                     null,
                     It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                 Times.Once);
        }

        #endregion
    }
}
