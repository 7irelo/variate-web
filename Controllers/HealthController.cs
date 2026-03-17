using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using variate.Data;

namespace variate.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public HealthController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var databaseStatus = "healthy";
        try
        {
            await _db.Database.CanConnectAsync();
        }
        catch
        {
            databaseStatus = "unhealthy";
        }

        var overallStatus = databaseStatus == "healthy" ? "healthy" : "degraded";
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        return Ok(new
        {
            status = overallStatus,
            timestamp = DateTime.UtcNow,
            version,
            services = new { database = databaseStatus }
        });
    }
}
