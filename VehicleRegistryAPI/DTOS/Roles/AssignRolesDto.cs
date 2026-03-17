using System.ComponentModel.DataAnnotations;

namespace VehicleRegistryAPI.DTOS.Roles
{
    public class AssignRolesDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public List<int> RoleIds { get; set; } = new List<int>();
    }
}
