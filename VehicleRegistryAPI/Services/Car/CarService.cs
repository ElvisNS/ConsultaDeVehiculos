using AutoMapper;
using VehicleRegistryAPI.DTOS;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.Repositories.Interfaces;

namespace VehicleRegistryAPI.Services.Car
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;

        public CarService(
            ICarRepository carRepository,
            IPersonRepository personRepository,
            IMapper mapper)
        {
            _carRepository = carRepository;
            _personRepository = personRepository;
            _mapper = mapper;
        }

        public async Task<PageResponse<CarResponseDto>> GetAllAsync(int page, int pageSize)
        {
            var (cars, totalRecords) = await _carRepository
                .GetPagedAsync(page, pageSize, c => c.Persons);

            var mappedCars = _mapper.Map<IEnumerable<CarResponseDto>>(cars);

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
            var person = await _personRepository
                .GetFirstOrDefaultAsync(p => p.NationalId == dto.Cedula);

            if (person == null)
                throw new Exception("Persona no encontrada");

            var car = _mapper.Map<VehicleRegistryAPI.Entities.Car>(dto);
            car.PersonId = person.Id;

            await _carRepository.AddAsync(car);

            return _mapper.Map<CarResponseDto>(car);
        }

        public async Task<CarResponseDto> UpdateAsync(int id, UpdateCarDto dto)
        {
            var car = await _carRepository.GetFirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                throw new Exception("Carro no encontrado");

            var person = await _personRepository
                .GetFirstOrDefaultAsync(p => p.NationalId == dto.Cedula);

            if (person == null)
                throw new Exception("Persona no encontrada");

            car.PersonId = person.Id;

            await _carRepository.UpdateAsync(car);

            return _mapper.Map<CarResponseDto>(car);
        }

        public async Task<CarResponseDto?> GetByIdAsync(int id)
        {
            var car = await _carRepository.GetFirstOrDefaultAsync(
                c => c.Id == id,
                c => c.Persons
            );

            if (car == null)
                return null;

            return _mapper.Map<CarResponseDto>(car);
        }

        public async Task<CarResponseDto?> GetByPlateNumberAsync(string plateNumber)
        {
            var car = await _carRepository.GetFirstOrDefaultAsync(
                c => c.PlateNumber == plateNumber,
                c => c.Persons
            );

            if (car == null)
                return null;

            return _mapper.Map<CarResponseDto>(car);
        }
    }
}
