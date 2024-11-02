using Microsoft.AspNetCore.Mvc;

namespace variate.Controllers
{
    [Route("access-denied")]
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}