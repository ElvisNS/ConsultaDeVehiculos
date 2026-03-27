using Microsoft.Extensions.Logging;
using Moq;
using VehicleRegistryAPI.DTOS.Roles;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Services.Roles;

namespace VehicleRegistryAPI.Tests.Services
{
    public class RoleServiceTests
    {
        private readonly Mock<IRolesRepository> _repositoryMock;
        private readonly Mock<ILogger<RoleService>> _mockLogger;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            _repositoryMock = new Mock<IRolesRepository>();
            _mockLogger = new Mock<ILogger<RoleService>>();
            _roleService = new RoleService(
                _repositoryMock.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task AssignRolesToUserAsync_WhenUserExistsAndRoleExistsAndNoCurrentRole_ShouldAddRole()
        {
            // Arrange
            var dto = new AssignRolesDto { UserId = 1, RoleId = 2 };
            _repositoryMock.Setup(r => r.UserExistsAsync(dto.UserId)).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.RoleExistsAsync(dto.RoleId)).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.GetByUserIdAsync(dto.UserId)).ReturnsAsync((UserRoles)null);

            // Act
            await _roleService.AssignRolesToUserAsync(dto);

            // Assert
            _repositoryMock.Verify(r => r.AddAsync(It.Is<UserRoles>(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId)), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _repositoryMock.Verify(r => r.Remove(It.IsAny<UserRoles>()), Times.Never);
        }

        [Fact]
        public async Task AssignRolesToUserAsync_WhenUserAlreadyHasSameRole_ShouldDoNothing()
        {
            // Arrange
            var dto = new AssignRolesDto { UserId = 1, RoleId = 2 };
            var existingRole = new UserRoles { UserId = 1, RoleId = 2 };

            _repositoryMock.Setup(r => r.UserExistsAsync(dto.UserId)).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.RoleExistsAsync(dto.RoleId)).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.GetByUserIdAsync(dto.UserId)).ReturnsAsync(existingRole);

            // Act
            await _roleService.AssignRolesToUserAsync(dto);

            // Assert
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<UserRoles>()), Times.Never);
            _repositoryMock.Verify(r => r.Remove(It.IsAny<UserRoles>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
        [Fact]
        public async Task AssignRolesToUserAsync_WhenUserHasDifferentRole_ShouldReplaceRole()
        {
            // Arrange
            var dto = new AssignRolesDto { UserId = 1, RoleId = 3 };
            var existingRole = new UserRoles { UserId = 1, RoleId = 2 };

            _repositoryMock.Setup(r => r.UserExistsAsync(dto.UserId)).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.RoleExistsAsync(dto.RoleId)).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.GetByUserIdAsync(dto.UserId)).ReturnsAsync(existingRole);

            // Act
            await _roleService.AssignRolesToUserAsync(dto);

            // Assert
            _repositoryMock.Verify(r => r.Remove(existingRole), Times.Once);
            _repositoryMock.Verify(r => r.AddAsync(It.Is<UserRoles>(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId)), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task AssignRolesToUserAsync_WhenUserDoesNotExist_ShouldThrowExceptionAndLogWarning()
        {
            // Arrange
            var dto = new AssignRolesDto { UserId = 99, RoleId = 1 };
            _repositoryMock.Setup(r => r.UserExistsAsync(dto.UserId)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _roleService.AssignRolesToUserAsync(dto));
            Assert.Contains("no existe", exception.Message);

            // Verificar log de warning
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Usuario 99 no encontrado")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        [Fact]
        public async Task AssignRolesToUserAsync_WhenRoleDoesNotExist_ShouldThrowExceptionAndLogWarning()
        {
            // Arrange
            var dto = new AssignRolesDto { UserId = 1, RoleId = 999 };
            _repositoryMock.Setup(r => r.UserExistsAsync(dto.UserId)).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.RoleExistsAsync(dto.RoleId)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _roleService.AssignRolesToUserAsync(dto));
            Assert.Contains("no existe", exception.Message);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Rol 999 no encontrado")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        [Fact]
        public async Task AssignRolesToUserAsync_WhenRepositoryThrowsException_ShouldLogErrorAndRethrow()
        {
            // Arrange
            var dto = new AssignRolesDto { UserId = 1, RoleId = 2 };
            var expectedException = new InvalidOperationException("DB error");

            _repositoryMock.Setup(r => r.UserExistsAsync(dto.UserId)).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.RoleExistsAsync(dto.RoleId)).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.GetByUserIdAsync(dto.UserId)).ReturnsAsync((UserRoles)null);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<UserRoles>())).ThrowsAsync(expectedException);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _roleService.AssignRolesToUserAsync(dto));
            Assert.Same(expectedException, ex);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al asignar rol al usuario 1")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
