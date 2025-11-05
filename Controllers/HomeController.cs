using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using test2.Data;
using test2.Models;
using Microsoft.EntityFrameworkCore;
using test2.Models.ViewModels;
using System.Security.Claims;

namespace test2.Controllers
{
    public class HomeController : Controller
    {
        private readonly test2Context _context;
        public HomeController(test2Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);

        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult BloodCamping()
        {
            var compingsWithHospitals = _context.BloodCompings
       .Include(b => b.Hospital)
       .Select(b => new BloodCompingViewModel
       {
           Id = b.Id,
           Type = b.Type,
           Amount = b.Amount,
           Goal = b.Goal,
           Hospital = _context.Hospitals.FirstOrDefault(h => h.Id == b.HospitalId)
       })
       .ToList();
            return View(compingsWithHospitals);
        }
        public IActionResult Categories(string id)
        {
            var camps = _context.Compings.Where(c => c.CategoryId == int.Parse(id)).ToList();
            if (camps == null) return RedirectToAction("Index", "Home");
            return View(camps);
        }
        public IActionResult Donate(int id)
        {
            ViewBag.CompingId = id;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Donate(UserDonationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
                return RedirectToAction("Login", "User");

            var userId = int.Parse(userClaim.Value);
            var donation = new Donation
            {
                CompingId = model.CompingId,
                UserId = userId,
                Amount = model.Amount,
            };

            _context.Add(donation);

            var comping = await _context.Compings.FindAsync(model.CompingId);
            if (comping != null)
            {
                comping.Amount += (int)model.Amount;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult DonationDetails(int id)
        {
            var cause = _context.Compings.Include(c => c.Category)
                .FirstOrDefault(c => c.Id == id);

            if (cause == null)
            {
                return RedirectToAction("Index", "Home"); // or show 404
            }

            return View(cause);
        }
        public IActionResult HospitalDetails(int id)
        {
            var hospital = _context.Hospitals.FirstOrDefault(h => h.Id == id);
            if (hospital == null) return RedirectToAction("Hospital", "Home");

            var compings = _context.BloodCompings
                .Where(b => b.HospitalId == id)
                .ToList();

            var viewModel = new HospitalWithCompingsViewModel
            {
                Hospital = hospital,
                BloodCompings = compings
            };

            return View(viewModel);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
