using VehicleRegistryAPI.DTOS;
using VehicleRegistryAPI.DTOS.Persons;

namespace VehicleRegistryAPI.Services.Person
{
    public interface IPersonService
    {
        Task<PersonResponseDto> CreateAsync(CreatePersonDto dto);
        Task<PersonResponseDto> UpdateAsync(int id, UpdatePersonDto dto);
        Task<PersonResponseDto> GetByIdAsync(int id);
        Task<PageResponse<PersonResponseDto>> GetAllAsync(int page, int pageSize);
        Task<bool> ExistsByNationalIdAsync(string nationalId);
        Task<PersonResponseDto> GetByNationalIdAsync(string nationalId);
        Task<PersonResponseDto> ToggleActive(int id);
    }
}
