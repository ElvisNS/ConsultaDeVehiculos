namespace VehicleRegistryAPI.DTOS.Cars
{
    public class CreateCarDto
    {
        public string PlateNumber { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Cedula { get; set; }  
    }
}
