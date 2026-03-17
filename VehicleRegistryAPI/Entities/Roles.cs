namespace VehicleRegistryAPI.Entities
{
    public class Roles 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<UserRoles> UserRoless { get; set; }
    }
}
