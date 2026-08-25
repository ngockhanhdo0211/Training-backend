using Microsoft.AspNetCore.Mvc;
namespace RecursiveCommentApi.Controllers;
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult GetHealth()
    {
        return Ok(new
        {
            Status = "Healthy",
            CheckedAtUtc = DateTime.UtcNow
        });
    }
}