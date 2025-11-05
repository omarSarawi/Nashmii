using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using test2.Data;
using test2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;



namespace test2.Controllers
{

    public class UsersController : Controller
    {
        private readonly test2Context _context;

        public UsersController(test2Context context)
        {
            _context = context;
        }

        // GET: Users

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }
        [HttpGet]
        [Authorize(Roles = "Admin,HospitalAdmin")]
        public async Task<IActionResult> Account()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Users");
            int userId = int.Parse(userIdClaim.Value);

            var user = await _context.Users
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Users");

            var cashDonations = await _context.Donations
                                              .Include(d => d.Comping)
                                              .Where(d => d.UserId == userId)
                                              .OrderByDescending(d => d.Id)          // assuming higher Id == newer
                                              .ToListAsync();

            var bloodDonations = await _context.BloodDonations
                                               .Include(b => b.BloodComping)
                                               .Where(b => b.UserId == userId)
                                               .OrderByDescending(b => b.Id)
                                               .ToListAsync();

            var rows = cashDonations.Select(d => new DonationRow
            {
                DonationId = d.Id,
                CampaignType = d.Comping?.Title ?? "Campaign",
                DonationAmount = (decimal)d.Amount,
                UnitType = "JOD"                     // or $, €, etc.
            })
                        .Concat(bloodDonations.Select(b => new DonationRow
                        {
                            DonationId = b.Id ?? 0,
                            CampaignType = "Blood",
                            DonationAmount = b.Amount ?? 0,
                            UnitType = "Units"
                        }))
                        .OrderByDescending(r => r.Date)
                        .Take(20)
                        .ToList();

            var vm = new ProfileViewModel
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                TotalDonationsCount = rows.Count,
                TotalDonationAmount = (decimal)cashDonations.Sum(d => d.Amount),
                TotalUnitsDonated = bloodDonations.Sum(b => b.Amount) ?? 0,
                LastDonationDate = rows.FirstOrDefault()?.Date,
                Donations = rows
            };

            return View(vm);

        }


        // GET: Users/Details/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        [Authorize(Roles = "Admin")]

        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Username,Email,Password,Phone")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Email,Password,Phone")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userCompings = _context.Compings.Where(c => c.UserId == id);
            _context.Compings.RemoveRange(userCompings);

            var userCategories = _context.Categories.Where(cat => cat.UserId == id);
            _context.Categories.RemoveRange(userCategories);
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            TempData["Success"] = "User and all related data deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [AllowAnonymous]

        public IActionResult Login(string? denied)
        {
            if (!string.IsNullOrEmpty(denied) && denied == "true")
            {
                ViewBag.AccessDeniedMessage = "Access denied. You do not have permission to view that page.";
            }

            return View();
        }

        // POST: Login
        [HttpPost]
        [AllowAnonymous]

        async public Task<IActionResult> Login(string username, string password)
        {
            // Find the user by username
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            try
            {
                if (user != null)
                {
                    var passwordHasher = new PasswordHasher<User>();
                    var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        User? userFromDatabase = _context.Users.SingleOrDefault(u => u.Username == user.Username);
                        if (userFromDatabase == null)
                        {
                            ModelState.AddModelError(String.Empty, "User is not in the database");
                            return View(user);
                        }
                        if (result != PasswordVerificationResult.Success)
                        {
                            ModelState.AddModelError(String.Empty, "password is not match");
                            return View(user);
                        }
                        await HttpContext.SignInAsync("default", new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {
                    new Claim(ClaimTypes.Name,userFromDatabase.Username),
                    new Claim(ClaimTypes.NameIdentifier,userFromDatabase.Id.ToString()),
                    new Claim(ClaimTypes.Role,userFromDatabase.Role)

                }, "default")));

                        // Redirect to homepage or dashboard
                        if (userFromDatabase.Role == Role.Admin.ToString())
                        {
                            return RedirectToAction("Index", "Categories");
                        }
                        else if (userFromDatabase.Role == Role.HospitalAdmin.ToString())
                        {
                            return RedirectToAction("Index", "HospitalDashboard");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                // If login fails
                ViewBag.Error = "username or password is not correct";
                return View();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("=======" + e.Message);

                throw;
            }
        }
        private bool CompairPassword(string hashedPassword, string inputPassword)
        {
            PasswordHasher<string> hasher = new();
            return hasher.VerifyHashedPassword(String.Empty, hashedPassword, inputPassword) == PasswordVerificationResult.Success;
        }
        [AllowAnonymous]

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("default");
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]

        // GET: Users/Register
        public IActionResult Register()
        {

            return View();
        }


        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]

        public async Task<IActionResult> Register([Bind("Username,Email,Password,Phone")] User user)
        {

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, user.Password);
            user.Role = "User";
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }




        // GET: Users/RegisterAdmin
        [AllowAnonymous]

        public IActionResult RegisterAdmin()
        {
            return View();
        }

        // POST: Users/RegisterAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]

        public async Task<IActionResult> RegisterAdmin([Bind("Username,Email,Password,Phone")] User user)
        {

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, user.Password);
            user.Role = "Admin";
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");

        }
        // GET: Users/RegisterAdminHospital

        // TODO : wtffffff!!!!!!!
        public IActionResult RegisterAdminHospital()
        {
            return View();
        }

        // POST: Users/RegisterAdminHospital
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdminHospital([Bind("Username,Email,Password,Phone")] User user)
        {


            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, user.Password);


            user.Role = "AdminHospital";


            _context.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");


        }
        [AllowAnonymous]

        public IActionResult AccessDeniedRedirect()
        {
            return Redirect("/Users/Login?denied=true");
        }
    }
}
