using Microsoft.AspNetCore.Mvc;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Services.Person;

namespace VehicleRegistryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;

        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }



        [HttpPost]
        public async Task<IActionResult> Create(CreatePersonDto createPersonDto)
        {
            var createdPerson = await _personService.CreateAsync(createPersonDto);
            return Ok(createdPerson);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UpdatePersonDto updatePersonDto)
        {
            var person = await _personService.GetByIdAsync(id);
            var UpdatedPerson = await _personService.UpdateAsync(person.Id, updatePersonDto);
            return Ok(UpdatedPerson);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 5;

            var result = await _personService.GetAllAsync(page, pageSize);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var person = await _personService.GetByIdAsync(id);
            return Ok(person);
        }

        [HttpGet("by-nationalid/{nationalId}")]
        public async Task<IActionResult> GetByNationalId(string nationalId)
        {
            var person = await _personService.GetByNationalIdAsync(nationalId);
            return Ok(person);
        }
    }
}