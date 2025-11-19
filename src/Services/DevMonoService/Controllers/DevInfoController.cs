using Microsoft.AspNetCore.Mvc;

namespace DevMonoService.Controllers;

[ApiController]
[Route("api/dev")]
[Tags("Development")]
public class DevInfoController : ControllerBase
{
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            Service = "Development Mono Service",
            Version = "1.0.0",
            Environment = "Development",
            Description = "Unified API for all services in development environment",
            Services = new[]
            {
                new { Name = "Identity Service", Path = "/api/identity", Description = "Authentication and user management" },
                new { Name = "Integration Hub", Path = "/api/integration", Description = "External system integrations" },
                new { Name = "Core Workflow Service", Path = "/api/workflow", Description = "Core business workflows" }
            },
            SwaggerUrl = "/swagger",
            HealthCheck = "/health"
        });
    }

    [HttpGet("services")]
    public IActionResult GetServices()
    {
        return Ok(new
        {
            AvailableServices = new[]
            {
                "Identity Service",
                "Integration Hub",
                "Core Workflow Service"
            },
            Note = "All endpoints are available through this unified interface for development purposes only"
        });
    }
}