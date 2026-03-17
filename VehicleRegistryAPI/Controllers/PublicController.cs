using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VehicleRegistryAPI.Services.Car;
using VehicleRegistryAPI.Services.Person;

namespace VehicleRegistryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly IPersonService _personService;
        public PublicController(ICarService carService, IPersonService personService)
        {
            _carService = carService;
            _personService = personService;

        }
        // ✅ GET: api/cars/by-plate/A123456
        [HttpGet("by-plate/{plate}")]
        public async Task<IActionResult> GetByPlate(string plate)
        {
            var car = await _carService.GetByPlateNumberAsync(plate);

            if (car == null)
                return NotFound("Carro no encontrado");

            return Ok(car);
        }

        // 000-0000000-0

        [HttpGet("by-nationalid/{nationalId}")]
        public async Task<IActionResult> GetByNationalId(string nationalId)
        {
            var person = await _personService.GetByNationalIdAsync(nationalId);
            return Ok(person);
        }
    }
}
