using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using VehicleRegistryAPI.DTOS.Auth;
using VehicleRegistryAPI.Services.Auth;

namespace VehicleRegistryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<LoginDto> _validator;
        public AuthController(IAuthService authService, IValidator<LoginDto> validator)
        {
            _authService = authService;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser(LoginDto loginDto)
        {
            var validationResult = await _validator.ValidateAsync(loginDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var result = await _authService.Login(loginDto);
            return Ok(result);
        }
    }
}
