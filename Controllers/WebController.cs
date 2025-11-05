using Microsoft.AspNetCore.Mvc;

namespace test2.Controllers
{
    public class WebController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
