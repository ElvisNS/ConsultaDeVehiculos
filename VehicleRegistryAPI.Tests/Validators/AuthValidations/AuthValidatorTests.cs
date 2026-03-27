using FluentValidation.Results;
using Moq;
using VehicleRegistryAPI.DTOS.Auth;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Tools.Security;
using VehicleRegistryAPI.Tools.Validations.AuthValidations;

namespace VehicleRegistryAPI.Tests.Validators.AuthValidations
{
    public class AuthValidatorTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly AuthValidator _validator;

        public AuthValidatorTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _validator = new AuthValidator(_userRepositoryMock.Object, _passwordHasherMock.Object);
        }

        private async Task<ValidationResult> ValidateAsync(LoginDto dto)
        {
            return await _validator.ValidateAsync(dto);
        }

        #region Email Validation

        [Fact]
        public async Task Should_HaveError_WhenEmailIsEmpty()
        {
            var dto = new LoginDto { Email = "", Password = "password123" };

            var result = await ValidateAsync(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "El email es requerido");
        }

        [Fact]
        public async Task Should_HaveError_WhenEmailIsInvalid()
        {
            var dto = new LoginDto { Email = "invalid-email", Password = "password123" };

            var result = await ValidateAsync(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Formato de email inválido");
        }

        [Fact]
        public async Task Should_NotHaveError_WhenEmailIsValid()
        {
            var dto = new LoginDto { Email = "test@example.com", Password = "password123" };

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            var result = await ValidateAsync(dto);

            Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Email");
        }

        #endregion

        #region Password Validation

        [Fact]
        public async Task Should_HaveError_WhenPasswordIsEmpty()
        {
            var dto = new LoginDto { Email = "test@example.com", Password = "" };

            var result = await ValidateAsync(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password" && e.ErrorMessage == "La contraseña es requerida");
        }

        [Fact]
        public async Task Should_NotHaveError_WhenPasswordIsProvided()
        {
            var dto = new LoginDto { Email = "test@example.com", Password = "password123" };

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            var result = await ValidateAsync(dto);

            Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Password");
        }

        #endregion

        #region Credentials Validation

        [Fact]
        public async Task Should_HaveError_WhenUserNotFound()
        {
            var dto = new LoginDto { Email = "nonexistent@example.com", Password = "password123" };

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            var result = await ValidateAsync(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Credenciales inválidas");
        }

        [Fact]
        public async Task Should_HaveError_WhenPasswordIsIncorrect()
        {
            var dto = new LoginDto { Email = "test@example.com", Password = "wrongpassword" };
            var user = new User
            {
                Id = 1,
                Email = dto.Email,
                PasswordHash = "hashedpassword",
                IsActive = true
            };

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(p => p.VerifyPassword(dto.Password, user.PasswordHash))
                .Returns(false);

            var result = await ValidateAsync(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Credenciales inválidas");
        }

        [Fact]
        public async Task Should_NotHaveError_WhenCredentialsAreValid()
        {
            var dto = new LoginDto { Email = "test@example.com", Password = "correctpassword" };
            var user = new User
            {
                Id = 1,
                Email = dto.Email,
                PasswordHash = "hashedpassword",
                IsActive = true
            };

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(p => p.VerifyPassword(dto.Password, user.PasswordHash))
                .Returns(true);

            var result = await ValidateAsync(dto);

            Assert.True(result.IsValid);
            Assert.DoesNotContain(result.Errors, e => e.ErrorMessage == "Credenciales inválidas");
        }

        #endregion

        #region User Active Validation

        [Fact]
        public async Task Should_HaveError_WhenUserIsInactive()
        {
            var dto = new LoginDto { Email = "test@example.com", Password = "correctpassword" };
            var user = new User
            {
                Id = 1,
                Email = dto.Email,
                PasswordHash = "hashedpassword",
                IsActive = false
            };

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(p => p.VerifyPassword(dto.Password, user.PasswordHash))
                .Returns(true);

            var result = await ValidateAsync(dto);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Usuario desactivado");
        }

        [Fact]
        public async Task Should_NotHaveError_WhenUserIsActive()
        {
            var dto = new LoginDto { Email = "test@example.com", Password = "correctpassword" };
            var user = new User
            {
                Id = 1,
                Email = dto.Email,
                PasswordHash = "hashedpassword",
                IsActive = true
            };

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(p => p.VerifyPassword(dto.Password, user.PasswordHash))
                .Returns(true);

            var result = await ValidateAsync(dto);

            Assert.True(result.IsValid);
            Assert.DoesNotContain(result.Errors, e => e.ErrorMessage == "Usuario desactivado");
        }

        #endregion
    }
}
