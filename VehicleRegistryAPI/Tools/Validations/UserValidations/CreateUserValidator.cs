using FluentValidation;
using Microsoft.EntityFrameworkCore.Metadata;
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
                .MinimumLength(3);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6);
        }
        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            return !await _userService.EmailExists(email);
        }
    }
}
