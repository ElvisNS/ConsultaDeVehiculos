using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PublicController> _logger;
        public PublicController(ICarService carService, IPersonService personService, ILogger<PublicController> logger)
        {
            _carService = carService;
            _personService = personService;
            _logger = logger;
        }
        // ✅ GET: api/cars/by-plate/A123456
        [HttpGet("by-plate/{plate}")]
        public async Task<IActionResult> GetByPlate(string plate)
        {
            var car = await _carService.GetByPlateNumberAsync(plate);

            if (car == null)
            {
                _logger.LogWarning("Vehiculo no encontrado con placa: {Plate}", plate);
                return NotFound("Carro no encontrado");
            }

            _logger.LogInformation("Vehiculo obtenido exitosamente con placa: {Plate}", plate);
            return Ok(car);
        }

        // 000-0000000-0

        [HttpGet("by-nationalid/{nationalId}")]
        public async Task<IActionResult> GetByNationalId(string nationalId)
        {
            var person = await _personService.GetByNationalIdAsync(nationalId);
            _logger.LogInformation("Persona obtenida con cedula: {NationalId}", nationalId);
            return Ok(person);
        }
    }
}
