using FluentValidation;
using VehicleRegistryAPI.DTOS.Auth;
using VehicleRegistryAPI.Repositories.Implementations;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Tools.Security;

namespace VehicleRegistryAPI.Tools.Validations.AuthValidations
{
    public class AuthValidator : AbstractValidator<LoginDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        public AuthValidator(IUserRepository userRepositor, IPasswordHasher passwordHasher ) 
        {
            _userRepository = userRepositor;
            _passwordHasher = passwordHasher;

            RuleFor(x => x.Email)
           .NotEmpty().WithMessage("El email es requerido")
           .EmailAddress().WithMessage("Formato de email inválido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida");

            RuleFor(x => x)
                .MustAsync(ValidateCredentials)
                .WithMessage("Credenciales inválidas");

            RuleFor(x => x)
                .MustAsync(UserIsActive)
                .WithMessage("Usuario desactivado");
        }
        private async Task<bool> ValidateCredentials(LoginDto dto, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null)
                return false;

            return _passwordHasher.VerifyPassword(dto.Password, user.PasswordHash);
        }

        private async Task<bool> UserIsActive(LoginDto dto, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null)
                return false;

            return user.IsActive;
        }
    }
}
