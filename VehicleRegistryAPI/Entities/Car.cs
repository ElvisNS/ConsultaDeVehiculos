using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace VehicleRegistryAPI.Entities
{
    public class Car : BaseEntity
    {
        public string PlateNumber { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }


        // Clave foránea
        [Required]
        public int PersonId { get; set; }

        // Propiedad de navegación
        public Person Persons { get; set; }

    }
}
