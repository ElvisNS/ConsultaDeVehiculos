using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Services.Person;
using VehicleRegistryAPI.Tools.Exceptions;

namespace VehicleRegistryAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;
        private readonly IValidator<CreatePersonDto> _createvalidator;
        private readonly IValidator<UpdatePersonDto> _updateValidator;
        private readonly ILogger<PersonController> _logger;
        public PersonController(IPersonService personService, 
            IValidator<CreatePersonDto> createvalidator, 
            IValidator<UpdatePersonDto> updatevalidator,
            ILogger<PersonController> logger)
        {
            _personService = personService;
            _createvalidator = createvalidator;
            _updateValidator = updatevalidator;
            _logger = logger;
        }



        [Authorize(Roles = "Admin, Operator")]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePersonDto createPersonDto)
        {
            var validationResult = await _createvalidator.ValidateAsync(createPersonDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Fallo la validacion al crear persona");
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var createdPerson = await _personService.CreateAsync(createPersonDto);
            _logger.LogInformation("Persona creada con ID: {PersonId}", createdPerson.Id);
            return Ok(createdPerson);
        }

        [Authorize(Roles = "Admin, Operator")]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UpdatePersonDto updatePersonDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(updatePersonDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Fallo la validacion al actualizar persona con ID: {PersonId}", id);
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var person = await _personService.GetByIdAsync(id);
            var UpdatedPerson = await _personService.UpdateAsync(person.Id, updatePersonDto);
            _logger.LogInformation("Persona actualizada con ID: {PersonId}", id);
            return Ok(UpdatedPerson);
        }

        [Authorize(Roles = "Admin, Operator")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 5;

            var result = await _personService.GetAllAsync(page, pageSize);
            _logger.LogInformation("GetAll personas llamado - Pagina: {Page}, TamanoPagina: {PageSize}", page, pageSize);

            return Ok(result);
        }

        [Authorize(Roles = "Admin, Operator")]
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _personService.ToggleActive(id);
            _logger.LogInformation("Persona desactivada con ID: {PersonId}", id);

            return Ok(result);
        }
    }
}