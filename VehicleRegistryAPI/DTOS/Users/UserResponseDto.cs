namespace VehicleRegistryAPI.DTOS.Users
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        public int? DeactivatedBy { get; set; }

        public bool IsActive { get; set; }
        public string? RoleName { get; set; }
    }
}
