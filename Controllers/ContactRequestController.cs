using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test2.Data;
using test2.Models;

namespace test2.Controllers
{
    [Authorize(Roles = "Admin")]

    public class ContactRequestController : Controller
    {
        private readonly test2Context _context;

        public ContactRequestController(test2Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var allRequests = await _context.ContactRequests.ToListAsync();
            return View(allRequests);
        }

        [AllowAnonymous]
        public IActionResult CreateContact()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Users");
            var hospital = _context.Hospitals.Where(e => e.OwnerUserId == int.Parse(userIdClaim.Value));
            if (hospital.Any())
                return RedirectToAction("Index", "HospitalDashboard");
            if (User.IsInRole("Admin")) return RedirectToAction("Index", "Dashboard");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]

        public async Task<IActionResult> CreateContact(ContactRequest request, IFormFile imageFile)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Users");
            }
            var hospital = _context.Hospitals.Where(e => e.OwnerUserId == int.Parse(userIdClaim.Value));
            if (hospital.Any())
                return RedirectToAction("Index", "HospitalDashboard");
            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Users");
            }

            request.OwnerReqId = userId;
            request.OwnerReqUser = user;
            request.Status = ContactStatus.Pending;
            request.CreatedAt = DateTime.Now;
            if (imageFile != null && imageFile.Length > 0)
            {
                var imageUploadService = new ImageUploadService();
                var imageUrl = await imageUploadService.UploadImageAsync(imageFile);
                if (imageUrl == null) return RedirectToAction(nameof(CreateContact));
                request.ImageURL = imageUrl;
            }
            if (ModelState.IsValid)
            {
                _context.ContactRequests.Add(request);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Your request has been submitted successfully!";
                return RedirectToAction("Index", "Home");
            }
            return View(request);
        }


        // TODO : make this filter for pending in view side rether then in the controller 
        public async Task<IActionResult> GetContectsPending()
        {
            var pendingRequests = await _context.ContactRequests
                .Where(r => r.Status == ContactStatus.Pending)
                .ToListAsync();
            return View(pendingRequests);
        }

        public async Task<IActionResult> ContectDetails(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.ContactRequests.FindAsync(id);
            if (request == null) return NotFound();

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveContect(int id)
        {
            var request = await _context.ContactRequests.Include(r => r.OwnerReqUser).FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Users");

            var hospital = new Hospital
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                OwnerUserId = request.OwnerReqId,
                ImageURL = request.ImageURL,
                ApprovedByUserId = int.Parse(userIdClaim.Value)
            };

            var user = await _context.Users.FindAsync(request.OwnerReqId);
            if (user == null) return RedirectToAction("Login", "Users");
            if (user.Role == Role.Admin.ToString()) return RedirectToAction("Login", "Users");
            user.Role = Role.HospitalAdmin.ToString();
            _context.Users.Update(user);
            _context.Hospitals.Add(hospital);

            request.Status = ContactStatus.Approved;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Hospital '{hospital.Name}' created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.ContactRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = ContactStatus.Rejected;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Request for '{request.Name}' has been rejected.";
            return RedirectToAction(nameof(GetContectsPending));
        }

        private bool ContactRequestExists(int id)
        {
            return _context.ContactRequests.Any(e => e.Id == id);
        }
    }
}
