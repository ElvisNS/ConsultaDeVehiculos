namespace VehicleRegistryAPI.DTOS.Cars
{
    public class CarResponseDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }

        public string Cedula { get; set; }
        public string Nombre { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeactivatedAt { get; set; }
    }
}
