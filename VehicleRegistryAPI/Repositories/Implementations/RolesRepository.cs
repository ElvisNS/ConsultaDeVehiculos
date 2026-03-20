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

        // Obtiene el rol de un usuario (ahora es uno solo)
        public async Task<UserRoles?> GetByUserIdAsync(int userId)
        {
            return await _context.Set<UserRoles>()
                                 .FirstOrDefaultAsync(ur => ur.UserId == userId);
        }

        // Agrega una asignación de rol (una sola)
        public async Task AddAsync(UserRoles userRole)
        {
            await _context.Set<UserRoles>().AddAsync(userRole);
        }

        // Elimina una asignación de rol
        public void Remove(UserRoles userRole)
        {
            _context.Set<UserRoles>().Remove(userRole);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        // Nuevo: verifica si un rol existe
        public async Task<bool> RoleExistsAsync(int roleId)
        {
            return await _context.Roles.AnyAsync(r => r.Id == roleId);
        }
    }
}
