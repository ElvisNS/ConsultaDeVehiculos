using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using VehicleRegistryAPI.DTOS.Auth;
using VehicleRegistryAPI.Services.Auth;
using Microsoft.Extensions.Logging;

namespace VehicleRegistryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<LoginDto> _validator;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, IValidator<LoginDto> validator, ILogger<AuthController> logger)
        {
            _authService = authService;
            _validator = validator;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser(LoginDto loginDto)
        {
            var validationResult = await _validator.ValidateAsync(loginDto);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Fallo la validacion de login para usuario: {Email}", loginDto.Email);
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var result = await _authService.Login(loginDto);
            _logger.LogInformation("Usuario inicio sesion exitosamente: {Email}", loginDto.Email);
            return Ok(result);
        }
    }
}
