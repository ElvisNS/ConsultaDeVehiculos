using FluentValidation.TestHelper;
using Moq;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.Services.Car;
using VehicleRegistryAPI.Services.Person;
using VehicleRegistryAPI.Tools.Validations.CarValidations;

namespace VehicleRegistryAPI.Tests.Validators.CarValidations
{
    public class CreateCarValidatorTests
    {
        private readonly Mock<ICarService> _carServiceMock;
        private readonly Mock<IPersonService> _personServiceMock;
        private readonly CreateCarValidator _validator;

        public CreateCarValidatorTests()
        {
            _carServiceMock = new Mock<ICarService>();
            _personServiceMock = new Mock<IPersonService>();

            _validator = new CreateCarValidator(
                _carServiceMock.Object,
                _personServiceMock.Object
            );
        }

        #region PlateNumber

        [Fact]
        public async Task Should_Fail_When_PlateNumber_Empty()
        {
            var dto = new CreateCarDto { PlateNumber = "", Brand = "Toyota", Model = "Spare", Cedula = "001-0000000-0" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.PlateNumber);
        }

        [Fact]
        public async Task Should_Fail_When_Plate_Invalid_Format()
        {
            var dto = new CreateCarDto { PlateNumber = "123", Brand = "Toyota", Model = "Spare", Cedula = "001-0000000-0" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.PlateNumber);
        }

        [Fact]
        public async Task Should_Fail_When_Plate_Already_Exists()
        {
            _carServiceMock
                .Setup(x => x.ExistsByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(true); // ya existe

            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "Toyota", Model = "Spare", Cedula = "001-0000000-0" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.PlateNumber)
                  .WithErrorMessage("Plate Number Ya existe");
        }

        [Fact]
        public async Task Should_Fail_When_PlateNumber_Exceeds_MaxLength()
        {
            var dto = new CreateCarDto { PlateNumber = "A1234567", Brand = "ABCDEFGHIJK", Model = "Spare", Cedula = "001-0000000-0" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.PlateNumber);
        }

        [Fact]
        public async Task Should_Pass_When_PlateNumber_Is_Exactly_MaxLength()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "ABCDEFGHIJ", Model = "Spare", Cedula = "001-0000000-0" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.PlateNumber);
        }

        #endregion

        #region Cedula

        [Fact]
        public async Task Should_Fail_When_Cedula_Empty()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "Toyota", Model = "Spare", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cedula);
        }

        [Fact]
        public async Task Should_Pass_When_Cedula_Isvalid()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "Toyota", Model = "Spare", Cedula = "001-1234567-8" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cedula);
        }

        [Fact]
        public async Task Should_Fail_When_Cedula_Invalid_Format()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "Toyota", Model = "Spare", Cedula = "123" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cedula);
        }

        [Fact]
        public async Task Should_Pass_When_Cedula_Isvalid_Format()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "Toyota", Model = "Spare", Cedula = "001-1234567-8" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cedula);
        }

        [Fact]
        public async Task Should_Fail_When_Person_Does_Not_Exist()
        {
            _personServiceMock
                .Setup(x => x.ExistsByNationalIdAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "Toyota", Model = "Spare", Cedula = "001-1234567-8" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Cedula)
                  .WithErrorMessage("Cedula no existe");
        }

        #endregion

        #region Model
        [Fact]
        public async Task Should_Fail_When_Model_Empty()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "Toyota", Model = "", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Fact]
        public async Task Should_Fail_When_Model_Invalid_Format()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "Toyota", Model = "Sp3132", Cedula = "123" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Fact]
        public async Task Should_Fail_When_Model_Less_Than_MinLength()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "ABCDEFGHIJ", Model = "Sp", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }
        [Fact]
        public async Task Should_Fail_When_Model_Exceeds_MaxLength()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "ABCDEFGHIJK", Model = "ABCDEFGHIJK", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Fact]
        public async Task Should_Pass_When_Model_Is_Exactly_MinLength()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "ABC", Model = "Spa", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Model);
        }

        [Fact]
        public async Task Should_Pass_When_Model_Is_Exactly_MaxLength()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "ABCDEFGHIJ", Model = "ABCDEFGHIJ", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Model);
        }
        #endregion

        #region Brand
        [Fact]
        public async Task Should_Fail_When_Brand_Empty()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "", Model = "Spare", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Brand);
        }
        [Fact]
        public async Task Should_Fail_When_Brand_Less_Than_MinLength()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "AB", Model = "Spare", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Brand);
        }
        [Fact]
        public async Task Should_Fail_When_Brand_Exceeds_MaxLength()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "ABCDEFGHIJK", Model = "Spare", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Brand);
        }

        [Fact]
        public async Task Should_Pass_When_Brand_Is_Exactly_MinLength()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "ABC", Model = "Spare", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Brand);
        }

        [Fact]
        public async Task Should_Pass_When_Brand_Is_Exactly_MaxLength()
        {
            var dto = new CreateCarDto { PlateNumber = "A123456", Brand = "ABCDEFGHIJ", Model = "Spare", Cedula = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.Brand);
        }
        #endregion

        [Fact]
        public async Task Should_Pass_When_All_Data_Is_Valid()
        {
            _carServiceMock
                .Setup(x => x.ExistsByPlateNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _personServiceMock
                .Setup(x => x.ExistsByNationalIdAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var dto = new CreateCarDto
            {
                PlateNumber = "A123456",
                Cedula = "001-1234567-8",
                Model = "Civic",
                Brand = "Honda"
            };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
