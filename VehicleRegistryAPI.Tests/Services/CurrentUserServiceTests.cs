using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using VehicleRegistryAPI.Services.Users;

namespace VehicleRegistryAPI.Tests.Services
{
    public class CurrentUserServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ILogger<CurrentUserService>> _mockLogger;
        private readonly CurrentUserService _currentUserService;

        public CurrentUserServiceTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockLogger = new Mock<ILogger<CurrentUserService>>();
            _currentUserService = new CurrentUserService(
                _mockHttpContextAccessor.Object,
                _mockLogger.Object);
        }

        [Fact]
        public void UserId_ConHttpContextNulo_RetornaNull()
        {
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            var result = _currentUserService.UserId;

            Assert.Null(result);
        }

        [Fact]
        public void UserId_SinUsuarioAutenticado_RetornaNull()
        {
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            var result = _currentUserService.UserId;

            Assert.Null(result);
        }

        [Fact]
        public void UserId_ConClaimNameIdentifierValido_RetornaUserId()
        {
            int expectedUserId = 123;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            var result = _currentUserService.UserId;

            Assert.NotNull(result);
            Assert.Equal(expectedUserId, result);
        }

        [Fact]
        public void UserId_ConClaimNameIdentifierInvalido_RetornaNull()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid-id")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            var result = _currentUserService.UserId;

            Assert.Null(result);
        }

        [Fact]
        public void UserId_ConClaimNameIdentifierVacio_RetornaNull()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            var result = _currentUserService.UserId;

            Assert.Null(result);
        }

        [Fact]
        public void UserId_ConMultipleClaims_ObtieneCorrectamenteElNameIdentifier()
        {
            int expectedUserId = 456;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            var result = _currentUserService.UserId;

            Assert.NotNull(result);
            Assert.Equal(expectedUserId, result);
        }
    }
}
