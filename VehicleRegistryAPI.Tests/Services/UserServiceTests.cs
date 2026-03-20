using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using VehicleRegistryAPI.DTOS.Users;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Services.Users;
using VehicleRegistryAPI.Tools.Exceptions;
using VehicleRegistryAPI.Tools.Security;

namespace VehicleRegistryAPI.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _userService = new UserService(
                _mockUserRepository.Object,
                _mockMapper.Object,
                _mockPasswordHasher.Object,
                _mockLogger.Object);
        }

        #region AddUser Tests

        [Fact]
        public async Task AddUser_ConDatosValidos_RetornaUserResponseDto()
        {
            var createDto = new CreateUserDto
            {
                Email = "test@example.com",
                UserName = "testuser",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = createDto.Email,
                UserName = createDto.UserName,
                PasswordHash = "hashedPassword"
            };

            var expectedResponse = new UserResponseDto
            {
                Id = 1,
                Email = createDto.Email,
                UserName = createDto.UserName,
                IsActive = true
            };

            _mockMapper.Setup(m => m.Map<User>(createDto)).Returns(user);
            _mockPasswordHasher.Setup(p => p.HashPassword(createDto.Password)).Returns("hashedPassword");
            _mockUserRepository.Setup(r => r.AddAsync(user)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(user)).Returns(expectedResponse);

            var result = await _userService.AddUser(createDto);

            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.Email, result.Email);
            Assert.Equal(expectedResponse.UserName, result.UserName);
        }

        [Fact]
        public async Task AddUser_VerificaHashPassword()
        {
            var createDto = new CreateUserDto
            {
                Email = "test@example.com",
                UserName = "testuser",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = createDto.Email,
                UserName = createDto.UserName
            };

            _mockMapper.Setup(m => m.Map<User>(createDto)).Returns(user);
            _mockPasswordHasher.Setup(p => p.HashPassword(createDto.Password)).Returns("hashedPassword");
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>())).Returns(new UserResponseDto());

            await _userService.AddUser(createDto);

            _mockPasswordHasher.Verify(p => p.HashPassword(createDto.Password), Times.Once);
            Assert.Equal("hashedPassword", user.PasswordHash);
        }

        [Fact]
        public async Task AddUser_VerificaLlamadaAddAsyncAlRepositorio()
        {
            var createDto = new CreateUserDto
            {
                Email = "test@example.com",
                UserName = "testuser",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = createDto.Email,
                UserName = createDto.UserName
            };

            _mockMapper.Setup(m => m.Map<User>(createDto)).Returns(user);
            _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashedPassword");
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>())).Returns(new UserResponseDto());

            await _userService.AddUser(createDto);

            _mockUserRepository.Verify(r => r.AddAsync(user), Times.Once);
        }

        [Fact]
        public async Task AddUser_VerificaMapeoCorrecto()
        {
            var createDto = new CreateUserDto
            {
                Email = "test@example.com",
                UserName = "testuser",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = createDto.Email,
                UserName = createDto.UserName
            };

            _mockMapper.Setup(m => m.Map<User>(createDto)).Returns(user);
            _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashedPassword");
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>())).Returns(new UserResponseDto());

            await _userService.AddUser(createDto);

            _mockMapper.Verify(m => m.Map<User>(createDto), Times.Once);
            _mockMapper.Verify(m => m.Map<UserResponseDto>(user), Times.Once);
        }

        [Fact]
        public async Task AddUser_ConEmailVacio_LanzaExcepcion()
        {
            var createDto = new CreateUserDto
            {
                Email = "",
                UserName = "testuser",
                Password = "password123"
            };

            var user = new User();

            _mockMapper.Setup(m => m.Map<User>(createDto)).Returns(user);
            _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashedPassword");
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>())).Returns(new UserResponseDto());

            await _userService.AddUser(createDto);

            _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task AddUser_ConPasswordVacio_UtilizaPasswordVacio()
        {
            var createDto = new CreateUserDto
            {
                Email = "test@example.com",
                UserName = "testuser",
                Password = ""
            };

            var user = new User
            {
                Id = 1,
                Email = createDto.Email,
                UserName = createDto.UserName
            };

            _mockMapper.Setup(m => m.Map<User>(createDto)).Returns(user);
            _mockPasswordHasher.Setup(p => p.HashPassword("")).Returns("emptyHash");
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>())).Returns(new UserResponseDto());

            await _userService.AddUser(createDto);

            _mockPasswordHasher.Verify(p => p.HashPassword(""), Times.Once);
        }

        [Fact]
        public async Task AddUser_ConEmailYaExistente_LanzaConflictException()
        {
            var createDto = new CreateUserDto
            {
                Email = "existing@example.com",
                UserName = "testuser",
                Password = "password123"
            };

            _mockUserRepository.Setup(r => r.ExistsByEmailAsync(createDto.Email)).ReturnsAsync(true);

            var exception = await Assert.ThrowsAsync<ConflictException>(() => _userService.AddUser(createDto));

            Assert.Contains(createDto.Email, exception.Message);
            _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AddUser_VerificaQueSeVerificaEmailNoExiste()
        {
            var createDto = new CreateUserDto
            {
                Email = "test@example.com",
                UserName = "testuser",
                Password = "password123"
            };

            var user = new User
            {
                Id = 1,
                Email = createDto.Email,
                UserName = createDto.UserName
            };

            _mockUserRepository.Setup(r => r.ExistsByEmailAsync(createDto.Email)).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<User>(createDto)).Returns(user);
            _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashedPassword");
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(It.IsAny<User>())).Returns(new UserResponseDto());

            await _userService.AddUser(createDto);

            _mockUserRepository.Verify(r => r.ExistsByEmailAsync(createDto.Email), Times.Once);
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_ConIdValido_RetornaUserResponseDto()
        {
            int userId = 1;
            var updateDto = new UpdateUserDto
            {
                UserName = "updateduser",
                Password = "newpassword123"
            };

            var existingUser = new User
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "olduser",
                PasswordHash = "oldHash",
                IsActive = true
            };

            var expectedResponse = new UserResponseDto
            {
                Id = userId,
                Email = existingUser.Email,
                UserName = updateDto.UserName,
                IsActive = true
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockUserRepository.Setup(r => r.UpdateAsync(existingUser)).Returns(Task.CompletedTask);
            _mockPasswordHasher.Setup(p => p.HashPassword(updateDto.Password)).Returns("newHashedPassword");
            _mockMapper.Setup(m => m.Map(updateDto, existingUser)).Returns(existingUser);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(existingUser)).Returns(expectedResponse);

            var result = await _userService.UpdateUser(userId, updateDto);

            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.Email, result.Email);
            Assert.Equal(expectedResponse.UserName, result.UserName);
        }

        [Fact]
        public async Task UpdateUser_ConIdInvalido_LanzaNotFoundException()
        {
            int userId = 999;
            var updateDto = new UpdateUserDto
            {
                UserName = "updateduser"
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _userService.UpdateUser(userId, updateDto));
        }

        [Fact]
        public async Task UpdateUser_ConPasswordNuevo_ActualizaPasswordHash()
        {
            int userId = 1;
            var updateDto = new UpdateUserDto
            {
                UserName = "testuser",
                Password = "newPassword123"
            };

            var existingUser = new User
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "olduser",
                PasswordHash = "oldHash"
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockUserRepository.Setup(r => r.UpdateAsync(existingUser)).Returns(Task.CompletedTask);
            _mockPasswordHasher.Setup(p => p.HashPassword(updateDto.Password)).Returns("newHashedPassword");
            _mockMapper.Setup(m => m.Map(updateDto, existingUser)).Returns(existingUser);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(existingUser)).Returns(new UserResponseDto());

            await _userService.UpdateUser(userId, updateDto);

            _mockPasswordHasher.Verify(p => p.HashPassword(updateDto.Password), Times.Once);
            Assert.Equal("newHashedPassword", existingUser.PasswordHash);
        }

        [Fact]
        public async Task UpdateUser_SinPassword_NoLlamaHashPassword()
        {
            int userId = 1;
            var updateDto = new UpdateUserDto
            {
                UserName = "updateduser",
                Password = ""
            };

            var existingUser = new User
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "olduser",
                PasswordHash = "existingHash"
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockUserRepository.Setup(r => r.UpdateAsync(existingUser)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map(updateDto, existingUser)).Returns(existingUser);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(existingUser)).Returns(new UserResponseDto());

            await _userService.UpdateUser(userId, updateDto);

            _mockPasswordHasher.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
            Assert.Equal("existingHash", existingUser.PasswordHash);
        }

        [Fact]
        public async Task UpdateUser_VerificaLlamadaUpdateAsync()
        {
            int userId = 1;
            var updateDto = new UpdateUserDto
            {
                UserName = "testuser"
            };

            var existingUser = new User
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "olduser"
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockUserRepository.Setup(r => r.UpdateAsync(existingUser)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map(updateDto, existingUser)).Returns(existingUser);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(existingUser)).Returns(new UserResponseDto());

            await _userService.UpdateUser(userId, updateDto);

            _mockUserRepository.Verify(r => r.UpdateAsync(existingUser), Times.Once);
        }

        #endregion

        #region ToggleActiveUser Tests

        [Fact]
        public async Task ToggleActiveUser_ConIdValido_CambiaEstadoYRetornaUserResponseDto()
        {
            int userId = 1;
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "testuser",
                IsActive = true
            };

            var expectedResponse = new UserResponseDto
            {
                Id = userId,
                Email = user.Email,
                UserName = user.UserName,
                IsActive = false
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(user)).Returns(expectedResponse);

            var result = await _userService.ToggleActiveUser(userId);

            Assert.NotNull(result);
            Assert.False(result.IsActive);
            Assert.False(user.IsActive);
        }

        [Fact]
        public async Task ToggleActiveUser_ConIdInvalido_LanzaNotFoundException()
        {
            int userId = 999;

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _userService.ToggleActiveUser(userId));
        }

        [Fact]
        public async Task ToggleActiveUser_DeActivoAInactivo_CambiaIsActiveAFalse()
        {
            int userId = 1;
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "testuser",
                IsActive = true
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(user)).Returns(new UserResponseDto { IsActive = false });

            await _userService.ToggleActiveUser(userId);

            Assert.False(user.IsActive);
        }

        [Fact]
        public async Task ToggleActiveUser_DeInactivoAActivo_CambiaIsActiveATrue()
        {
            int userId = 1;
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "testuser",
                IsActive = false
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(user)).Returns(new UserResponseDto { IsActive = true });

            await _userService.ToggleActiveUser(userId);

            Assert.True(user.IsActive);
        }

        [Fact]
        public async Task ToggleActiveUser_VerificaLlamadaUpdateAsync()
        {
            int userId = 1;
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                UserName = "testuser",
                IsActive = true
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserRepository.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(user)).Returns(new UserResponseDto());

            await _userService.ToggleActiveUser(userId);

            _mockUserRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        #endregion
    }
}
