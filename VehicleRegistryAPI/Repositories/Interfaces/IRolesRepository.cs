using VehicleRegistryAPI.Entities;

namespace VehicleRegistryAPI.Repositories.Interfaces
{
    public interface IRolesRepository
    {
        Task<IEnumerable<UserRoles>> GetByUserIdAsync(int userId);
        Task AddRangeAsync(IEnumerable<UserRoles> userRoles);
        void RemoveRange(IEnumerable<UserRoles> userRoles);
        Task SaveChangesAsync();
        Task<bool> UserExistsAsync(int userId);
        Task<IEnumerable<int>> GetExistingRoleIdsAsync(IEnumerable<int> roleIds);
    }
}
