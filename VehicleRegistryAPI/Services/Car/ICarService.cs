using VehicleRegistryAPI.DTOS;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.DTOS.Persons;

namespace VehicleRegistryAPI.Services.Car
{
    public interface ICarService
    {
        Task<CarResponseDto?> GetByPlateNumberAsync(string plateNumber);

        Task<CarResponseDto?> GetByIdAsync(int id);
        Task<CarResponseDto> CreateAsync(CreateCarDto dto);
        Task<CarResponseDto> UpdateAsync(int id, UpdateCarDto dto);
        //Task<CarResponseDto> GetByIdAsync(int id);
        Task<PageResponse<CarResponseDto>> GetAllAsync(int page, int pageSize);
        //Task<CarResponseDto> GetByPlateNumberAsync(string plateNumber);
    }
}
            