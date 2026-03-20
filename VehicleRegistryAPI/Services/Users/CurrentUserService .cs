using System.Security.Claims;

namespace VehicleRegistryAPI.Services.Users
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentUserService> _logger;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public int? UserId
        {
            get
            {
                // Registrar el intento de obtener el usuario
                _logger.LogDebug("Intentando obtener el ID del usuario actual");


                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("No hay HttpContext disponible (ejecución fuera de una petición web)");
                    return null;
                }

                var userIdClaim = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("No se encontró el claim NameIdentifier en el usuario. ¿Usuario no autenticado?");
                    return null;
                }

                if (int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogDebug("Usuario autenticado con ID {UserId}", userId);
                    return userId;
                }
                else
                {
                    _logger.LogError("El claim NameIdentifier tiene un valor no numerico: {ClaimValue}", userIdClaim);
                    return null;
                }
            }
        }
    }
}
