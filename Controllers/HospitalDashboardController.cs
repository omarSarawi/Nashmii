using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using test2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using test2.Models;

namespace test2.Controllers
{
    [Authorize(Roles = "HospitalAdmin")]

    public class HospitalDashboardController : Controller
    {
        private readonly test2Context _context;

        public HospitalDashboardController(test2Context context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Users");
            var hospital = _context.Hospitals.FirstOrDefault(h => h.OwnerUserId == int.Parse(userIdClaim.Value));
            if (hospital == null) return NotFound();
            var bloodDonations = _context.BloodCompings.Where(b => b.HospitalId == hospital.Id).ToList();
            return View(bloodDonations);
        }
        public async Task<IActionResult> Donors()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Users");

            var userId = int.Parse(userIdClaim.Value);

            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.OwnerUserId == userId);
            if (hospital == null) return NotFound("No hospital found for the current user.");

            var donors = await _context.BloodDonations
                .Where(d => d.BloodComping.HospitalId == hospital.Id)
                .Include(d => d.User)
                .Include(d => d.BloodComping)
                .ToListAsync();

            return View(donors);
        }
        [HttpGet("HospitalDashboard/BloodCompingDonors/{BloodCompingId}")]
        public async Task<IActionResult> BloodCompingDonors(int BloodCompingId)
        {
            System.Console.WriteLine($"{BloodCompingId} : " + BloodCompingId.GetType() + "==========");
            if (BloodCompingId <= 0) return RedirectToAction("Index", "HospitalDashboard");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Users");

            var userId = int.Parse(userIdClaim.Value);

            var donors = await _context.BloodDonations
                .Where(d => d.BloodComping.Id == BloodCompingId)
                .Include(d => d.User)
                .Include(d => d.BloodComping)
                .ToListAsync();

            return View(donors);
        }
        [HttpPost]
        public async Task<IActionResult> ApproveDonation(int donationId)
        {
            var donation = await _context.BloodDonations
                .Include(d => d.BloodComping)
                .FirstOrDefaultAsync(d => d.Id == donationId);

            if (donation == null) return RedirectToAction("Index", "HospitalDashboard");
            var bloodCompingId = donation.BloodComping?.Id;
            // TODO: add these to enums
            donation.Status = "Approved";


            var bloodComping = await _context.BloodCompings.FindAsync(bloodCompingId);
            if (bloodComping != null)
            {
                bloodComping.Amount += 1;
                _context.Entry(bloodComping).Property(b => b.Amount).IsModified = true;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(BloodCompingDonors), new { BloodCompingId = donation.BloodCompingId });
        }
        [HttpPost]
        public async Task<IActionResult> RejectDonation(int donationId)
        {
            var donation = await _context.BloodDonations.FindAsync(donationId);
            if (donation == null) return RedirectToAction("Index", "HospitalDashboard");
            // TODO: add these to enums
            donation.Status = "Rejected";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(BloodCompingDonors), new { BloodCompingId = donation.BloodCompingId });
        }


    }
}
