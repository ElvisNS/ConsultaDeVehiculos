using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Services.Car;

namespace VehicleRegistryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly ICarService _carService;
        public CarController(ICarService carService)
        {
            _carService = carService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCarDto createCarDto)
        {
            var createdCar = await _carService.CreateAsync(createCarDto);
            return Ok(createdCar);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateCarDto updateCarDto)
        {
            var car = await _carService.GetByIdAsync(id);
            var UpdatedCar = await _carService.UpdateAsync(car.Id, updateCarDto);
            return Ok(UpdatedCar);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 5;

            var result = await _carService.GetAllAsync(page, pageSize);

            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cars = await _carService.GetByIdAsync(id);
            return Ok(cars);
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

        
    }
}
