using VehicleRegistryAPI.DTOS.Roles;
using VehicleRegistryAPI.Entities;
using VehicleRegistryAPI.Repositories.Interfaces;

namespace VehicleRegistryAPI.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly IRolesRepository _rolesRepository;
        private readonly ILogger<RoleService> _logger;
        public RoleService(IRolesRepository rolesRepository, ILogger<RoleService> logger) 
        {
            _rolesRepository = rolesRepository;
            _logger = logger;
        }

        public async Task AssignRolesToUserAsync(AssignRolesDto dto)
        {
            _logger.LogInformation("Asignando rol {RoleId} al usuario {UserId}", dto.RoleId, dto.UserId);

            try
            {
                    // 1. Validar que el usuario existe
                    var userExists = await _rolesRepository.UserExistsAsync(dto.UserId);
                    if (!userExists)
                    {
                        _logger.LogWarning("Usuario {UserId} no encontrado", dto.UserId);
                        throw new Exception($"El usuario con ID {dto.UserId} no existe.");
                    }

                    // 2. Validar que el rol existe
                    var roleExists = await _rolesRepository.RoleExistsAsync(dto.RoleId);
                    if (!roleExists)
                    {
                        _logger.LogWarning("Rol {RoleId} no encontrado", dto.RoleId);
                        throw new Exception($"El rol con ID {dto.RoleId} no existe.");
                    }

                    // 3. Obtener el rol actual del usuario (si tiene)
                    var currentRole = await _rolesRepository.GetByUserIdAsync(dto.UserId);

                    if (currentRole != null)
                    {
                        // Si ya tiene el mismo rol, no hacemos nada
                        if (currentRole.RoleId == dto.RoleId)
                        {
                            _logger.LogInformation("El usuario {UserId} ya tiene el rol {RoleId}. No se requieren cambios.",
                                dto.UserId, dto.RoleId);
                            return;
                        }

                        // Si tiene un rol diferente, lo eliminamos
                        _logger.LogInformation("Eliminando rol anterior {OldRoleId} del usuario {UserId}",
                            currentRole.RoleId, dto.UserId);
                        _rolesRepository.Remove(currentRole);
                    }

                    // 4. Asignar el nuevo rol
                    var newUserRole = new UserRoles { UserId = dto.UserId, RoleId = dto.RoleId };
                    await _rolesRepository.AddAsync(newUserRole);
                    await _rolesRepository.SaveChangesAsync();

                    _logger.LogInformation("Rol {RoleId} asignado correctamente al usuario {UserId}",
                        dto.RoleId, dto.UserId);
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, "Error al asignar rol al usuario {UserId}", dto.UserId);
               throw;
            }
                
        }
    }
}
