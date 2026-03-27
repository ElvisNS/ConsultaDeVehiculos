using FluentValidation.TestHelper;
using Moq;
using VehicleRegistryAPI.DTOS.Users;
using VehicleRegistryAPI.Services.Users;
using VehicleRegistryAPI.Tools.Validations.UserValidations;

namespace VehicleRegistryAPI.Tests.Validators.UserValidations
{
    public class CreateUserValidatorTests
    {
        private readonly Mock<IUserService> _service;
        private readonly CreateUserValidator _validator;

        public CreateUserValidatorTests()
        {
            _service = new Mock<IUserService>();
            _validator = new CreateUserValidator(
                _service.Object
                );
        }

        #region Email
        [Fact]
        public async Task Should_Fail_When_Email_Empty()
        {
            var dto = new CreateUserDto { Email = "", UserName = "Juan Martinez", Password = "123456" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public async Task Should_Fail_When_Email_Invalid()
        {
            var dto = new CreateUserDto { Email = "test#test,com", UserName = "Juan Martinez", Password = "123456" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public async Task Should_Fail_When_Email_Already_Exists()
        {
             _service
                .Setup(x => x.EmailExists(It.IsAny<string>()))
                .ReturnsAsync(true); //ya existe

            var dto = new CreateUserDto { Email = "test@test.com", UserName = "Juan Martinez", Password = "123456" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("El email ya está registrado");
        }
        #endregion

        #region UserName
        [Fact]
        public async Task Should_Fail_When_UserName_Empty()
        {
            var dto = new CreateUserDto { Email = "test@test.com", UserName = "", Password = "123456" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public async Task Should_Fail_When_UserName_Invalid()
        {
            var dto = new CreateUserDto { Email = "test@test.com", UserName = "@#@#@!", Password = "123456" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public async Task Should_Fail_When_UserName_Less_Than_MinLength()
        {
            var dto = new CreateUserDto { Email = "test@test.com", UserName = "ab", Password = "123456" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public async Task Should_Fail_When_UserName_MaxLength()
        {
            var dto = new CreateUserDto { Email = "", UserName = "Juan Martinezdfdfaddsa", Password = "123456" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }
        #endregion

        #region Password
        [Fact]
        public async Task Should_Fail_When_Password_Empty()
        {
            var dto = new CreateUserDto { Email = "test@test.com", UserName = "juan", Password = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public async Task Should_Fail_When_Password_Invalid()
        {
            var dto = new CreateUserDto { Email = "test@test.com", UserName = "juan", Password = "juan@#@!" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public async Task Should_Fail_When_Password_Less_Than_MinLength()
        {
            var dto = new CreateUserDto { Email = "test@test.com", UserName = "juan", Password = "Ju1an" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        #endregion

    }
}
