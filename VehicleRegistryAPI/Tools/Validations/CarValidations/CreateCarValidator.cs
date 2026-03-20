using FluentValidation;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.Services.Car;
using VehicleRegistryAPI.Services.Person;
using VehicleRegistryAPI.Tools.Validations.ValidationHelpers;

namespace VehicleRegistryAPI.Tools.Validations.CarValidations
{
    public class CreateCarValidator : AbstractValidator<CreateCarDto>
    {
        private readonly ICarService _carService;
        private readonly IPersonService _personService;

        public CreateCarValidator(ICarService carService, 
            IPersonService personService) 
        {
            _carService = carService;
            _personService = personService;


            RuleFor(x => x.PlateNumber)
                .NotEmpty()
                .MaximumLength(7)
                .MustAsync(BeUniquePlateNumber).WithMessage("{PropertyName} Ya existe")
                .Must(HelpersValidate.IsValidPlate)
                .WithMessage("{PropertyName} debe tener el formato $000000 Ejemplo A123456");

            RuleFor(x => x.Cedula)
             .NotEmpty()
             .Must(HelpersValidate.IsValidNationalId)
             .WithMessage("{PropertyName} debe tener el formato 000-0000000-0")
             .MustAsync(PersonMustExist)
             .WithMessage("{PropertyName} no existe");

            RuleFor(x => x.Model)
                .NotEmpty()
                .WithMessage("{PropertyName} No puede estar vacia")
                .MinimumLength(3)
                .MaximumLength (10)
                .Must(HelpersValidate.IsValidName).WithMessage("{PropertyName} solo acepta letras");

            RuleFor(x => x.Brand)
                .NotEmpty()
                .WithMessage("{PropertyName} No puede estar vacia")
                .MinimumLength(3)
                .MaximumLength(10);
        }
        private async Task<bool> BeUniquePlateNumber(string plateNumber, CancellationToken cancellationToken)
        {
            // true = válido, false = ya existe
            return !await _carService.ExistsByPlateNumberAsync(plateNumber);
        }

        private async Task<bool> PersonMustExist(string nationalId, CancellationToken cancellationToken)
        {
            return await _personService.ExistsByNationalIdAsync(nationalId);
        }
    }
}
