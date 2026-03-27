using FluentValidation.TestHelper;
using Moq;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.Services.Person;
using VehicleRegistryAPI.Tools.Validations.CarValidations;

namespace VehicleRegistryAPI.Tests.Validators.CarValidations
{
    public class UpdateCarValidatorTests
    {
        private readonly Mock<IPersonService> _personServiceMock;
        private readonly UpdateCarValidator _validator;

        public UpdateCarValidatorTests()
        {
            _personServiceMock = new Mock<IPersonService>();

            _validator = new UpdateCarValidator(
                _personServiceMock.Object
            );
        }

        #region Cedula

        [Fact]
        public async Task Should_Fail_When_Cedula_Empty()
        {
            var dto = new UpdateCarDto { Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cedula);
        }

        [Fact]
        public async Task Should_Pass_When_Cedula_Isvalid()
        {
            var dto = new UpdateCarDto { Cedula = "001-1234567-8" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cedula);
        }

        [Fact]
        public async Task Should_Fail_When_Cedula_Invalid_Format()
        {
            var dto = new UpdateCarDto { Cedula = "123" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cedula);
        }

        [Fact]
        public async Task Should_Pass_When_Cedula_Isvalid_Format()
        {
            var dto = new UpdateCarDto { Cedula = "001-1234567-8" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cedula);
        }

        [Fact]
        public async Task Should_Fail_When_Person_Does_Not_Exist()
        {
            _personServiceMock
                .Setup(x => x.ExistsByNationalIdAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var dto = new UpdateCarDto { Cedula = "001-1234567-8" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cedula)
                  .WithErrorMessage("Cedula no existe");
        }

        #endregion
    }
}
