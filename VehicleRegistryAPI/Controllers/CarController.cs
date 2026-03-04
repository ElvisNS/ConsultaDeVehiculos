using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Services.Car;
using VehicleRegistryAPI.Tools.Validations;

namespace VehicleRegistryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly IValidator<CreateCarDto> _carCreateValidator;
        private readonly IValidator<UpdateCarDto> _carUpdateValidator;
        public CarController(ICarService carService, 
            IValidator<CreateCarDto> createValidator, 
            IValidator<UpdateCarDto> updateValidator)
        {
            _carService = carService;
            _carCreateValidator = createValidator;
            _carUpdateValidator = updateValidator;

        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCarDto createCarDto)
        {
            var validationResult = await _carCreateValidator.ValidateAsync(createCarDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var createdCar = await _carService.CreateAsync(createCarDto);
            return Ok(createdCar);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateCarDto updateCarDto)
        {
            var car = await _carService.GetByIdAsync(id);

            var validationResult = await _carUpdateValidator.ValidateAsync(updateCarDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

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
