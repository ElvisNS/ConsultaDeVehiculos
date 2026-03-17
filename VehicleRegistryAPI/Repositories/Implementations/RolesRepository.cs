using Microsoft.EntityFrameworkCore;
using VehicleRegistryAPI.Data;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;

namespace VehicleRegistryAPI.Repositories.Implementations
{
    public class RolesRepository : IRolesRepository
    {
        private readonly ApplicationDbContext _context;

        public RolesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserRoles>> GetByUserIdAsync(int userId)
        {
            return await _context.Set<UserRoles>()
                                 .Where(ur => ur.UserId == userId)
                                 .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<UserRoles> userRoles)
        {
            await _context.Set<UserRoles>().AddRangeAsync(userRoles);
        }

        public void RemoveRange(IEnumerable<UserRoles> userRoles)
        {
            _context.Set<UserRoles>().RemoveRange(userRoles);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        public async Task<IEnumerable<int>> GetExistingRoleIdsAsync(IEnumerable<int> roleIds)
        {
            return await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync();
        }
    }
}
