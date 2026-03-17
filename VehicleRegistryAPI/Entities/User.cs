namespace VehicleRegistryAPI.Entities
{
    public class User : BaseEntity
    {
        public string? Email { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public List<UserRoles> UserRoless { get; set; }
    }
}
