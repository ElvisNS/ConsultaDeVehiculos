using FluentValidation.TestHelper;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Tools.Validations.PersonValidations;

namespace VehicleRegistryAPI.Tests.Validators.PersonValidations
{
    public class UpdatePersonValidatorTests
    {
        private readonly UpdatePersonValidator _validator;

        public UpdatePersonValidatorTests()
        {
            _validator = new UpdatePersonValidator();
        }


        #region FullName 

        [Fact]
        public async Task Should_Fail_When_FullName_Empty()
        {
            var dto = new UpdatePersonDto { FullName = "" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public async Task Should_Fail_When_FullName_InvalidFormat()
        {
            var dto = new UpdatePersonDto { FullName = "643" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public async Task Should_Fail_When_FullName_Less_Than_MinLength()
        {
            var dto = new UpdatePersonDto { FullName = "64" };


            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }
        [Fact]
        public async Task Should_Fail_When_FullName_Exceeds_MaxLength()
        {
            var dto = new UpdatePersonDto { FullName = "01234567890123456789065" };

            var result = await _validator.TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }


        #endregion
    }
}
