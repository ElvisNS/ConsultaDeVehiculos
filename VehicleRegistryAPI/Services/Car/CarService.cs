using AutoMapper;
using VehicleRegistryAPI.DTOS;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Tools.Exceptions;

namespace VehicleRegistryAPI.Services.Car
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CarService> _logger;

        public CarService(
            ICarRepository carRepository,
            IPersonRepository personRepository,
            IMapper mapper,
            ILogger<CarService> logger)
        {
            _carRepository = carRepository;
            _personRepository = personRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PageResponse<CarResponseDto>> GetAllAsync(int page, int pageSize)
        {
            _logger.LogInformation("Obteniendo vehículos paginados: página {Page}, tamaño {PageSize}", page, pageSize);

            var (cars, totalRecords) = await _carRepository
                .GetPagedAsync(page, pageSize, c => c.IsActive, c => c.Persons);

            var mappedCars = _mapper.Map<IEnumerable<CarResponseDto>>(cars);

            _logger.LogInformation("Se obtuvieron {Count} vehículos de un total de {TotalRecords}",
                mappedCars.Count(), totalRecords);

            return new PageResponse<CarResponseDto>
            {
                Data = mappedCars,
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize
            };
        }
        public async Task<CarResponseDto> CreateAsync(CreateCarDto dto)
        {
            _logger.LogInformation("Creando nuevo vehículo con placa {PlateNumber} para cédula {Cedula}",
                           dto.PlateNumber, dto.Cedula);

            var person = await _personRepository
                .GetFirstOrDefaultAsync(p => p.NationalId == dto.Cedula);

            if (person == null)
            {
                _logger.LogWarning("No se encontró persona con cédula {Cedula} para asociar al vehículo", dto.Cedula);
                throw new NotFoundException("Persona no encontrada");
            }

            var car = _mapper.Map<VehicleRegistryAPI.Entities.Car>(dto);
            car.PersonId = person.Id;

            await _carRepository.AddAsync(car);

            _logger.LogInformation("Vehículo creado con ID {CarId}, placa {PlateNumber}, asociado a persona {PersonId}",
                car.Id, car.PlateNumber, person.Id);

            return _mapper.Map<CarResponseDto>(car);
        }
        public async Task<CarResponseDto> UpdateAsync(int id, UpdateCarDto dto)
        {
            _logger.LogInformation("Actualizando vehículo con ID {CarId}", id);

            var car = await _carRepository.GetFirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                _logger.LogWarning("Vehículo con ID {CarId} no encontrado para actualizar", id);
                throw new NotFoundException("Carro no encontrado");
            }

            var person = await _personRepository
                .GetFirstOrDefaultAsync(p => p.NationalId == dto.Cedula);

            if (person == null)
            {
                _logger.LogWarning("No se encontró persona con cédula {Cedula} para reasignar el vehículo {CarId}",
                    dto.Cedula, id);
                throw new NotFoundException("Persona no encontrada");
            }

            car.PersonId = person.Id;

            _mapper.Map(dto, car); 

            await _carRepository.UpdateAsync(car);

            _logger.LogInformation("Vehículo con ID {CarId} actualizado, ahora asociado a persona {PersonId}",
                id, person.Id);

            return _mapper.Map<CarResponseDto>(car);
        }
        public async Task<CarResponseDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("Buscando vehículo por ID {CarId}", id);

            var car = await _carRepository.GetFirstOrDefaultAsync(
                c => c.Id == id,
                c => c.Persons
            );

            if (car == null)
            {
                _logger.LogWarning("Vehículo con ID {CarId} no encontrado", id);
                throw new NotFoundException("Carro no encontrado");
            }

            _logger.LogInformation("Vehículo con ID {CarId} encontrado, placa {PlateNumber}", id, car.PlateNumber);
            return _mapper.Map<CarResponseDto>(car);
        }
        public async Task<bool> ExistsByPlateNumberAsync(string plateNumber)
        {
            _logger.LogDebug("Verificando existencia de vehículo con placa {PlateNumber}", plateNumber);

            var exists = await _carRepository.AnyAsync(c => c.PlateNumber == plateNumber);

            _logger.LogDebug("Resultado de existencia para placa {PlateNumber}: {Exists}", plateNumber, exists);

            return exists;
        }
        public async Task<CarResponseDto> GetByPlateNumberAsync(string plateNumber)
        {
            _logger.LogInformation("Buscando vehículo por placa {PlateNumber}", plateNumber);

            var car = await _carRepository.GetFirstOrDefaultAsync(
                c => c.PlateNumber == plateNumber,
                c => c.Persons
            );

            if (car == null)
            {
                _logger.LogWarning("Vehículo con placa {PlateNumber} no encontrado", plateNumber);
                throw new NotFoundException("Carro no encontrado");
            }

            _logger.LogInformation("Vehículo con placa {PlateNumber} encontrado (ID {CarId})", plateNumber, car.Id);
            return _mapper.Map<CarResponseDto>(car);
        }
        public async Task<CarResponseDto> ToggleActive(int id)
        {
            _logger.LogInformation("Cambiando estado activo de vehículo con ID {CarId}", id);

            var car = await _carRepository
               .GetFirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
            {
                _logger.LogWarning("Vehículo con ID {CarId} no encontrado para cambiar estado", id);
                throw new NotFoundException("Carro no encontrado");
            }

            car.IsActive = !car.IsActive;
            var nuevoEstado = car.IsActive ? "activo" : "inactivo";

            await _carRepository.UpdateAsync(car);

            _logger.LogInformation("Vehículo con ID {CarId} ahora está {Estado}", id, nuevoEstado);

            return _mapper.Map<CarResponseDto>(car);
        }
    }
}
