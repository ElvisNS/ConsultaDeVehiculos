using VehicleRegistryAPI.DTOS.Cars;

namespace VehicleRegistryAPI.DTOS.Persons
{
    public class PersonResponseDto
    {
        public int Id { get; set; }
        public string NationalId { get; set; } 
        public string FullName { get; set; } 

        public List<CarDto> Cars { get; set; } = new();
    }
}
