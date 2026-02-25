using Microsoft.AspNetCore.Mvc;

namespace variate.Controllers
{
    [Route("access-denied")]
    public class AccessDeniedController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}