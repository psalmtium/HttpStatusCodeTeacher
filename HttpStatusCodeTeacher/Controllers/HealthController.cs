using Microsoft.AspNetCore.Mvc;

namespace HttpStatusCodeTeacher.Controllers;

/// <summary>
/// Controller for health check endpoint
/// </summary>
[ApiController]
[Route("api/v1")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint to verify API status.
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            message = "HTTP Status Code Teacher API is running smoothly",
            version = "1.0.0"
        });
    }
}
