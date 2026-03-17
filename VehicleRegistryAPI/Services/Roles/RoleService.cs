using VehicleRegistryAPI.DTOS.Roles;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;

namespace VehicleRegistryAPI.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly IRolesRepository _rolesRepository;
        public RoleService(IRolesRepository rolesRepository) 
        {
            _rolesRepository = rolesRepository;
        }

        public async Task AssignRolesToUserAsync(AssignRolesDto dto)
        {
            // 1. Validar que el usuario existe
            var userExists = await _rolesRepository.UserExistsAsync(dto.UserId);
            if (!userExists)
                throw new Exception($"El usuario con ID {dto.UserId} no existe.");

            // 2. Validar que todos los roles existen
            var existingRoleIds = await _rolesRepository.GetExistingRoleIdsAsync(dto.RoleIds);
            var missingRoles = dto.RoleIds.Except(existingRoleIds).ToList();
            if (missingRoles.Any())
                throw new Exception($"Los siguientes roles no existen: {string.Join(", ", missingRoles)}");

            // 3. Obtener roles actuales del usuario
            var currentUserRoles = await _rolesRepository.GetByUserIdAsync(dto.UserId);
            var currentRoleIds = currentUserRoles.Select(ur => ur.RoleId).ToList();

            // 4. Eliminar roles que ya no están en la nueva lista
            var rolesToRemove = currentUserRoles.Where(ur => !dto.RoleIds.Contains(ur.RoleId)).ToList();
            if (rolesToRemove.Any())
                _rolesRepository.RemoveRange(rolesToRemove);

            // 5. Agregar nuevos roles que no tenía
            var rolesToAdd = dto.RoleIds
                .Where(roleId => !currentRoleIds.Contains(roleId))
                .Select(roleId => new UserRoles { UserId = dto.UserId, RoleId = roleId })
                .ToList();

            if (rolesToAdd.Any())
                await _rolesRepository.AddRangeAsync(rolesToAdd);

            // 6. Guardar cambios
            await _rolesRepository.SaveChangesAsync();
        }
    }
}
