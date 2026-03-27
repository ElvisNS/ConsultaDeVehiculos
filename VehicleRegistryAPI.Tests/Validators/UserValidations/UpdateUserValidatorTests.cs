using FluentValidation.TestHelper;
using VehicleRegistryAPI.DTOS.Users;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Tools.Validations.UserValidations;

namespace VehicleRegistryAPI.Tests.Validators.UserValidations
{
    public class UpdateUserValidatorTests
    {
        private readonly UpdateUserValidator _validator;

        public UpdateUserValidatorTests()
        {
            _validator = new UpdateUserValidator();
        }

        #region Username
        [Fact]
        public async Task Should_Fail_When_UserName_Empty()
        {
            var dto = new UpdateUserDto { UserName = "", Password = "123456" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public async Task Should_Fail_When_UserName_Less_Than_MinLength()
        {
            var dto = new UpdateUserDto { UserName = "ab", Password = "123456" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }
        #endregion

        #region Password
        [Fact]
        public void Debe_tener_error_cuando_la_contraseña_tiene_menos_de_6_caracteres()
        {
            // Arrange
            var dto = new UpdateUserDto { UserName = "juab", Password = "1234" };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage("La contraseña debe tener al menos 6 caracteres");
        }
        [Fact]
        public void No_debe_tener_error_cuando_la_contraseña_tiene_exactamente_6_caracteres()
        {
            var dto = new UpdateUserDto { UserName = "juab", Password = "123456" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void No_debe_tener_error_cuando_la_contraseña_tiene_más_de_6_caracteres()
        {
            var dto = new UpdateUserDto { UserName = "juab", Password = "123456" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void No_debe_tener_error_cuando_la_contraseña_es_null()
        {
            var dto = new UpdateUserDto { UserName = "juab", Password = null };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void No_debe_tener_error_cuando_la_contraseña_es_vacía()
        {
            var dto = new UpdateUserDto { UserName = "juab", Password = "" };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void No_debe_tener_error_cuando_la_contraseña_contiene_solo_espacios()
        {
            var dto = new UpdateUserDto { UserName = "juab", Password = "        " };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }
        #endregion
    }
}
