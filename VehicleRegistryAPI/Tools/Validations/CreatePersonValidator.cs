using FluentValidation;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Services.Person;
using VehicleRegistryAPI.Tools.Validations.ValidationHelpers;

namespace VehicleRegistryAPI.Tools.Validations
{
    public class CreatePersonValidator : AbstractValidator<CreatePersonDto>
    {
        private readonly IPersonService _personService;
        public CreatePersonValidator(IPersonService personService) 
        {
            _personService = personService;


            RuleFor(x => x.NationalId)
                .NotEmpty()
                .MaximumLength(13)
                .Must(Helpers.IsValidNationalId)
                .WithMessage("{PropertyName} debe tener el formato 000-0000000-0")
                .MustAsync(BeUniqueNationalId)
                .WithMessage("{PropertyName} ya existe");
            RuleFor(x => x.FullName)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(20)
                .Must(Helpers.IsValidName).WithMessage("{PropertyName} deben ser todas letras");

        }
        private async Task<bool> BeUniqueNationalId(string nationalId, CancellationToken cancellationToken)
        {
            // true = válido, false = ya existe
            return !await _personService.ExistsByNationalIdAsync(nationalId);
        }

    }
}

