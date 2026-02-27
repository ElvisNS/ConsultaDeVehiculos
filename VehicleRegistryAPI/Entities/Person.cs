namespace VehicleRegistryAPI.Entities
{
    public class Person : BaseEntity
    {
        public string NationalId { get; set; }
        public string FullName { get; set; }


        // Propiedad de navegación
        public ICollection<Car> Cars { get; set; }

    }
}
