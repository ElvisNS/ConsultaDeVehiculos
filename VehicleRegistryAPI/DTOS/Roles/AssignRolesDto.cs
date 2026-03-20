using System.ComponentModel.DataAnnotations;

namespace VehicleRegistryAPI.DTOS.Roles
{
    public class AssignRolesDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}
