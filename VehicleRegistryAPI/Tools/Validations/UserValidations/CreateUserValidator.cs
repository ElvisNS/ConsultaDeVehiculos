using FluentValidation;
using VehicleRegistryAPI.DTOS.Users;
using VehicleRegistryAPI.Services.Users;

namespace VehicleRegistryAPI.Tools.Validations.UserValidations
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        private readonly IUserService _userService;
        public CreateUserValidator(IUserService userService)
        {
            _userService = userService;

            RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("Formato inválido")
            .MustAsync(BeUniqueEmail).WithMessage("El email ya está registrado");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El username es requerido")
                .MinimumLength(3)
                .MaximumLength(10)
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Solo letras, números y guión bajo");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6)
                .Matches(@"[A-Z]").WithMessage("Debe contener al menos una mayúscula")
                .Matches(@"[0-9]").WithMessage("Debe contener al menos un número");
        }
        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            return !await _userService.EmailExists(email);
        }
    }
}
