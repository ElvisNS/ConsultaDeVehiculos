using FluentValidation.TestHelper;
using Moq;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Services.Person;
using VehicleRegistryAPI.Tools.Validations.PersonValidations;

namespace VehicleRegistryAPI.Tests.Validators.PersonValidations
{
    public class CreatePersonValidatorTests
    {
        private readonly Mock<IPersonService> _personService;
        private readonly CreatePersonValidator _validator;

        public CreatePersonValidatorTests()
        {
            _personService = new Mock<IPersonService>();
            _validator = new CreatePersonValidator(
                _personService.Object
                );
        }

        #region NationalId
        [Fact]
        public async Task Should_Fail_When_Cedula_Empty()
        {
            var dto = new CreatePersonDto { NationalId = "", FullName = "JOrge" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.NationalId);
        }

        [Fact]
        public async Task Should_Fail_When_Cedula_Invalid_Format()
        {
            var dto = new CreatePersonDto { NationalId = "123", FullName = "JOrge" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.NationalId);
        }


        [Fact]
        public async Task Should_Fail_When_National_Already_Exists()
        {
            _personService
                .Setup(x => x.ExistsByNationalIdAsync(It.IsAny<string>()))
                .ReturnsAsync(true); // ya existe

            var dto = new CreatePersonDto { NationalId = "001-1234567-8", FullName = "JOrge" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.NationalId)
                  .WithErrorMessage("National Id ya existe");
        }
        #endregion

        #region FullName 

        [Fact]
        public async Task Should_Fail_When_FullName_Empty()
        {
            var dto = new CreatePersonDto { NationalId = "001-1234567-8", FullName = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public async Task Should_Fail_When_FullName_InvalidFormat()
        {
            var dto = new CreatePersonDto { NationalId = "001-1234567-8", FullName = "643" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public async Task Should_Fail_When_FullName_Less_Than_MinLength()
        {
            var dto = new CreatePersonDto { NationalId = "001-1234567-8", FullName = "64" };


            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }
        [Fact]
        public async Task Should_Fail_When_FullName_Exceeds_MaxLength()
        {
            var dto = new CreatePersonDto { NationalId = "001-1234567-8", FullName = "01234567890123456789065" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }


        #endregion
    }
}
