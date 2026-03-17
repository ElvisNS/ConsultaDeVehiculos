namespace VehicleRegistryAPI.DTOS.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }

        public DateTime Expiration { get; set; }
    }
}
