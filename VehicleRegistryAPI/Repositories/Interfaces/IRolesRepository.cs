using VehicleRegistryAPI.Entities;

namespace VehicleRegistryAPI.Repositories.Interfaces
{
    public interface IRolesRepository
    {
        Task<UserRoles?> GetByUserIdAsync(int userId);
        Task AddAsync(UserRoles userRole);
        void Remove(UserRoles userRole);
        Task SaveChangesAsync();
        Task<bool> UserExistsAsync(int userId);
        Task<bool> RoleExistsAsync(int roleId);
    }
}
