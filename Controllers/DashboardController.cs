using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace test2.Controllers
{
    public class DashboardController : Controller
    {
        [Authorize(AuthenticationSchemes = "default", Roles = "Admin")]
        public ActionResult Index()
        {

            return View();
        }

    }
}
