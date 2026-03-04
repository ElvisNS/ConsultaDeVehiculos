using FluentValidation;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Tools.Validations.ValidationHelpers;

namespace VehicleRegistryAPI.Tools.Validations
{
    public class UpdatePersonValidator : AbstractValidator<UpdatePersonDto>
    { 
        public UpdatePersonValidator() 
        { 
            RuleFor(x => x.FullName).NotEmpty()
                .MinimumLength(3)
                .MaximumLength(20)
                .Must(Helpers.IsValidName).WithMessage("{PropertyName} deben ser todas letras");
        }
    }
}
