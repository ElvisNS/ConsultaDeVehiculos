namespace VehicleRegistryAPI.Entities
{
    public class UserRoles
    {
        public int UserId { get; set; }

        public User Users { get; set; }


        public int RoleId { get; set; }

        public Roles Role { get; set; }
    }
}
