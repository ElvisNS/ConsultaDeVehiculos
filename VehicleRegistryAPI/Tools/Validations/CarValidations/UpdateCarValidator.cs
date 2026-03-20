using FluentValidation;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.Services.Person;
using VehicleRegistryAPI.Tools.Validations.ValidationHelpers;

namespace VehicleRegistryAPI.Tools.Validations.CarValidations
{
    public class UpdateCarValidator : AbstractValidator<UpdateCarDto>
    {
        private readonly IPersonService _personService;

        public UpdateCarValidator(IPersonService personService) 
        {
            _personService = personService;


            RuleFor(x => x.Cedula)
             .NotEmpty()
             .Must(HelpersValidate.IsValidNationalId)
             .WithMessage("{PropertyName} debe tener el formato 000-0000000-0")
             .MustAsync(PersonMustExist)
             .WithMessage("{PropertyName} no existe");
        }
        private async Task<bool> PersonMustExist(string nationalId, CancellationToken cancellationToken)
        {
            return await _personService.ExistsByNationalIdAsync(nationalId);
        }
    }
}
