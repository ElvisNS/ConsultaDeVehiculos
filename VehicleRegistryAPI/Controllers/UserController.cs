using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using VehicleRegistryAPI.DTOS.Roles;
using VehicleRegistryAPI.DTOS.Users;
using VehicleRegistryAPI.Services.Roles;
using VehicleRegistryAPI.Services.Users;

namespace VehicleRegistryAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IValidator<CreateUserDto> _createValidator;
        private readonly IValidator<UpdateUserDto> _updateValidator;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService userService, IRoleService roleService, IValidator<CreateUserDto> createValidator, IValidator<UpdateUserDto> updateValidator, ILogger<UserController> logger)
        {
            _userService = userService;
            _roleService = roleService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }


        [Authorize]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateUserDto updateUserDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(updateUserDto);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Fallo la validacion al actualizar usuario con ID: {UserId}", id);
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && currentUserId != id)
            {
                _logger.LogWarning("Usuario {CurrentUserId} intento editar usuario {TargetUserId} sin permiso", currentUserId, id);
                return Forbid("No tienes permiso para editar este usuario");
            }

            var users = await _userService.GetById(id);
            var UpdatedUsers = await _userService.UpdateUser(users.Id, updateUserDto);
            _logger.LogInformation("Usuario actualizado con ID: {UserId}", id);
            return Ok(UpdatedUsers);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _userService.ToggleActiveUser(id);
            _logger.LogInformation("Usuario desactivado con ID: {UserId}", id);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userService.GetAll();
            _logger.LogInformation("GetAll usuarios llamado ");
            return Ok(users);
        }

        [Authorize]
        [HttpGet("id")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != id)
            {
                _logger.LogWarning("Usuario {CurrentUserId} intento ver usuario {TargetUserId} sin permiso", currentUserId, id);
                return Forbid("No tienes permiso para ver este usuario");
            }
            var users = await _userService.GetById(id);
            _logger.LogInformation("Usuario obtenido con ID: {UserId}", id);
            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
        {
            var validationResult = await _createValidator.ValidateAsync(createUserDto);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Fallo la validacion al crear usuario");
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var createUser = await _userService.AddUser(createUserDto);
            _logger.LogInformation("Usuario creado con ID: {UserId}", createUser.Id);
            return Ok(createUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoles(AssignRolesDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _roleService.AssignRolesToUserAsync(dto);
                _logger.LogInformation("Roles asignados al usuario: {UserId}", dto.UserId);
                return Ok("Roles asignados correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar roles al usuario: {UserId}", dto.UserId);
                return BadRequest(ex.Message);
            }
        }
    }
}
