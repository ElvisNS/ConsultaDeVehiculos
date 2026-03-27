using VehicleRegistryAPI.DTOS;
using VehicleRegistryAPI.DTOS.Cars;

namespace VehicleRegistryAPI.Services.Car
{
    public interface ICarService
    {
        Task<CarResponseDto> GetByPlateNumberAsync(string plateNumber);

        Task<CarResponseDto> GetByIdAsync(int id);
        Task<CarResponseDto> CreateAsync(CreateCarDto dto);
        Task<CarResponseDto> UpdateAsync(int id, UpdateCarDto dto);

        Task<bool> ExistsByPlateNumberAsync(string plateNumber);

        Task<PageResponse<CarResponseDto>> GetAllAsync(int page, int pageSize);

        Task<CarResponseDto> ToggleActive(int id);

    }
}
