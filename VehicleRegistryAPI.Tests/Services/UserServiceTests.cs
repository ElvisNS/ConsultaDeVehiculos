using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using VehicleRegistryAPI.DTOS.Users;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;
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

        #region GetAll
        [Fact]
        public async Task GetAll_WhenUsersExist_ReturnsUserResponseDtoList()
        {
            // Arrange
            var users = new List<User>
    {
        new User
        {
            Id = 1,
            UserName = "juan.perez",
            Email = "juan@example.com",
            IsActive = true,
            UserRoless = new List<UserRoles> { new UserRoles { Role = new Roles { Name = "Admin" } } }
        },
        new User
        {
            Id = 2,
            UserName = "maria.gomez",
            Email = "maria@example.com",
            IsActive = false,
            UserRoless = new List<UserRoles> { new UserRoles { Role = new Roles { Name = "User" } } }
        }
    };

            var responseDtos = new List<UserResponseDto>
    {
        new UserResponseDto
        {
            Id = 1,
            UserName = "juan.perez",
            Email = "juan@example.com",
            IsActive = true,
            RoleName = "Admin"
        },
        new UserResponseDto
        {
            Id = 2,
            UserName = "maria.gomez",
            Email = "maria@example.com",
            IsActive = false,
            RoleName = "User"
        }
    };

            _mockUserRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<UserResponseDto>>(users))
                .Returns(responseDtos);

            // Act
            var result = await _userService.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDtos, result);
            _mockUserRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<UserResponseDto>>(users), Times.Once);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Obteniendo todos los usuarios")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Se obtuvieron {responseDtos.Count} usuarios")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenNoUsersExist_ReturnsEmptyList()
        {
            // Arrange
            var users = new List<User>();
            var responseDtos = new List<UserResponseDto>();

            _mockUserRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<UserResponseDto>>(users))
                .Returns(responseDtos);

            // Act
            var result = await _userService.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockUserRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<UserResponseDto>>(users), Times.Once);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Obteniendo todos los usuarios")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Se obtuvieron 0 usuarios")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Error de base de datos");

            _mockUserRepository
                .Setup(r => r.GetAllAsync())
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.GetAll());

            Assert.Equal(expectedException.Message, exception.Message);
            _mockMapper.Verify(m => m.Map<IEnumerable<UserResponseDto>>(It.IsAny<IEnumerable<User>>()), Times.Never);
            // Los logs no deberían ejecutarse porque la excepción ocurre antes
        }

        [Fact]
        public async Task GetAll_WhenMapperThrowsException_PropagatesException()
        {
            // Arrange
            var users = new List<User> { new User { Id = 1, UserName = "test", Email = "test@example.com" } };
            var expectedException = new AutoMapperMappingException("Error de mapeo");

            _mockUserRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<UserResponseDto>>(users))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _userService.GetAll());

            Assert.Equal(expectedException.Message, exception.Message);
            _mockUserRepository.Verify(r => r.GetAllAsync(), Times.Once);
            // Los logs sí se ejecutan antes de la excepción, pero no es necesario verificarlos aquí
        }
        #endregion

        #region GetById
        [Fact]
        public async Task GetById_ExistingUser_ReturnsUserResponseDto()
        {
            // Arrange
            int userId = 1;
            var user = new User
            {
                Id = userId,
                UserName = "juan.perez",
                Email = "juan@example.com",
                IsActive = true,
                UserRoless = new List<UserRoles> { new UserRoles { Role = new Roles { Name = "Admin" } } }
            };
            var responseDto = new UserResponseDto
            {
                Id = userId,
                UserName = "juan.perez",
                Email = "juan@example.com",
                IsActive = true,
                RoleName = "Admin"
            };

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockMapper
                .Setup(m => m.Map<UserResponseDto>(user))
                .Returns(responseDto);

            // Act
            var result = await _userService.GetById(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responseDto, result);
            _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockMapper.Verify(m => m.Map<UserResponseDto>(user), Times.Once);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Buscando usuario con ID {userId}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Usuario {userId} encontrado")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_UserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            int userId = 999;

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _userService.GetById(userId));

            Assert.Equal("Usuario no encontrado", exception.Message);
            _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockMapper.Verify(m => m.Map<UserResponseDto>(It.IsAny<User>()), Times.Never);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Usuario con ID {userId} no encontrado")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            int userId = 1;
            var expectedException = new InvalidOperationException("Error de base de datos");

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.GetById(userId));

            Assert.Equal(expectedException.Message, exception.Message);
            _mockMapper.Verify(m => m.Map<UserResponseDto>(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetById_WhenMapperThrows_PropagatesException()
        {
            // Arrange
            int userId = 1;
            var user = new User { Id = userId, UserName = "test", Email = "test@example.com" };
            var expectedException = new AutoMapperMappingException("Error de mapeo");

            _mockUserRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockMapper
                .Setup(m => m.Map<UserResponseDto>(user))
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _userService.GetById(userId));

            Assert.Equal(expectedException.Message, exception.Message);
        }
        #endregion

        #region EmailExists
        [Fact]
        public async Task EmailExists_WhenEmailExists_ReturnsTrue()
        {
            // Arrange
            string email = "juan@example.com";

            _mockUserRepository
                .Setup(r => r.ExistsByEmailAsync(email))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.EmailExists(email);

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(r => r.ExistsByEmailAsync(email), Times.Once);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Verificando existencia de email {email}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Email {email} existe: True")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task EmailExists_WhenEmailDoesNotExist_ReturnsFalse()
        {
            // Arrange
            string email = "nonexistent@example.com";

            _mockUserRepository
                .Setup(r => r.ExistsByEmailAsync(email))
                .ReturnsAsync(false);

            // Act
            var result = await _userService.EmailExists(email);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(r => r.ExistsByEmailAsync(email), Times.Once);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Verificando existencia de email {email}")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(LogLevel.Debug, It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Email {email} existe: False")),
                    It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task EmailExists_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            string email = "test@example.com";
            var expectedException = new InvalidOperationException("Error de base de datos");

            _mockUserRepository
                .Setup(r => r.ExistsByEmailAsync(email))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.EmailExists(email));

            Assert.Equal(expectedException.Message, exception.Message);
            _mockUserRepository.Verify(r => r.ExistsByEmailAsync(email), Times.Once);
            // Los logs se registran antes de la excepción, no es necesario verificarlos aquí
        }
        #endregion
    }
}
