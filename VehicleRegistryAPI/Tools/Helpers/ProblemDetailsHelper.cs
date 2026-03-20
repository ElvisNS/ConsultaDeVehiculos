using Microsoft.AspNetCore.Mvc;

namespace VehicleRegistryAPI.Tools.Helpers
{
    public static class ProblemDetailsHelper
    {
        public static ProblemDetails Create(HttpContext context, int status, string title, string detail)
        {
            return new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path,
                Extensions =
            {
                ["traceId"] = context.TraceIdentifier,
                ["timestamp"] = DateTime.UtcNow
            }
            };
        }
    }
}
