using VehicleRegistryAPI.DTOS.Roles;

namespace VehicleRegistryAPI.Services.Roles
{
    public interface IRoleService
    {
        Task AssignRolesToUserAsync(AssignRolesDto dto);
    }
}
