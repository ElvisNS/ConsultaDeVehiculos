using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Services.Car;
using VehicleRegistryAPI.Tools.Validations;

namespace VehicleRegistryAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly ICarService _carService;
        private readonly IValidator<CreateCarDto> _carCreateValidator;
        private readonly IValidator<UpdateCarDto> _carUpdateValidator;
        private readonly ILogger<CarController> _logger;
        public CarController(ICarService carService, 
            IValidator<CreateCarDto> createValidator, 
            IValidator<UpdateCarDto> updateValidator,
            ILogger<CarController> logger)
        {
            _carService = carService;
            _carCreateValidator = createValidator;
            _carUpdateValidator = updateValidator;
            _logger = logger;
        }

        [Authorize(Roles = "Admin, Operator")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateCarDto createCarDto)
        {
            var validationResult = await _carCreateValidator.ValidateAsync(createCarDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Fallo la validacion al crear vehiculo");
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var createdCar = await _carService.CreateAsync(createCarDto);
            _logger.LogInformation("Vehiculo creado con ID: {CarId}", createdCar.Id);
            return Ok(createdCar);
        }

        [Authorize(Roles = "Admin, Operator")]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateCarDto updateCarDto)
        {
            var car = await _carService.GetByIdAsync(id);

            var validationResult = await _carUpdateValidator.ValidateAsync(updateCarDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Fallo la validacion al actualizar vehiculo con ID: {CarId}", id);
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var UpdatedCar = await _carService.UpdateAsync(car.Id, updateCarDto);
            _logger.LogInformation("Vehiculo actualizado con ID: {CarId}", id);
            return Ok(UpdatedCar);
        }

        [Authorize(Roles = "Admin, Operator")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 5;

            var result = await _carService.GetAllAsync(page, pageSize);
            _logger.LogInformation("GetAll vehiculos llamado - Pagina: {Page}, TamanoPagina: {PageSize}", page, pageSize);

            return Ok(result);
        }

        [Authorize(Roles = "Admin, Operator")]
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _carService.ToggleActive(id);
            _logger.LogInformation("Vehiculo desactivado con ID: {CarId}", id);

            return Ok(result);
        }

    }
}
