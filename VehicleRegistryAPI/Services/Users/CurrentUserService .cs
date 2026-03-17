using System.Security.Claims;

namespace VehicleRegistryAPI.Services.Users
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                // Obtén el claim que contiene el ID numérico del usuario
                // Puede ser "sub", "nameidentifier" o el que uses
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }

                // Si no hay usuario autenticado o el claim no es un número válido, retorna null
                // Puedes cambiar esto por un valor por defecto como 0 si lo prefieres
                return null;
            }
        }
    }
}
