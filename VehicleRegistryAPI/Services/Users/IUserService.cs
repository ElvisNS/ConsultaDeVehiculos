using VehicleRegistryAPI.DTOS.Users;

namespace VehicleRegistryAPI.Services.Users
{
    public interface IUserService
    {
        Task<UserResponseDto> AddUser(CreateUserDto dto);
        Task<IEnumerable<UserResponseDto>> GetAll();
        Task<UserResponseDto> UpdateUser(int id, UpdateUserDto dto);
        Task<bool> EmailExists(string email);
        Task<UserResponseDto> ToggleActiveUser(int id);
        Task<UserResponseDto> GetById(int id);
    }
}
