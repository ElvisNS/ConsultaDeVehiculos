using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public UserController(IUserService userService, IRoleService roleService, IValidator<CreateUserDto> createValidator, IValidator<UpdateUserDto> updateValidator)
        {
            _userService = userService;
            _roleService = roleService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }


        [Authorize]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateUserDto updateUserDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(updateUserDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && currentUserId != id)
            {
                return Forbid("No tienes permiso para editar este usuario");
            }

            var users = await _userService.GetById(id);
            var UpdatedUsers = await _userService.UpdateUser(users.Id, updateUserDto);
            return Ok(UpdatedUsers);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _userService.ToggleActiveUser(id);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userService.GetAll();
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
                return Forbid("No tienes permiso para ver este usuario");
            }
            var users = await _userService.GetById(id);
            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
        {
            var validationResult = await _createValidator.ValidateAsync(createUserDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var createUser = await _userService.AddUser(createUserDto);
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
                return Ok("Roles asignados correctamente.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
