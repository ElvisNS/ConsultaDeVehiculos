using FluentValidation;
using VehicleRegistryAPI.DTOS.Users;

namespace VehicleRegistryAPI.Tools.Validations.UserValidations
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserValidator() 
        {
            RuleFor(x => x.UserName)
                     .NotEmpty().WithMessage("El username es requerido")
                     .MinimumLength(3);

            RuleFor(x => x.Password)
                .MinimumLength(6)
                .When(x => !string.IsNullOrWhiteSpace(x.Password))
                .WithMessage("La contraseña debe tener al menos 6 caracteres");
        }
    }
}
