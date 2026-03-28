using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using VehicleRegistryAPI.Controllers;
using VehicleRegistryAPI.DTOS.Auth;
using VehicleRegistryAPI.Services.Auth;

namespace VehicleRegistryAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IValidator<LoginDto>> _validatorMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _validatorMock = new Mock<IValidator<LoginDto>>();
            _loggerMock = new Mock<ILogger<AuthController>>();

            _controller = new AuthController(
                _authServiceMock.Object,
                _validatorMock.Object,
                _loggerMock.Object);
        }


    }
}
