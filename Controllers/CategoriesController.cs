using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using test2.Data;
using test2.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Security.Claims;

namespace test2.Controllers
{
    [Authorize(Roles = "Admin")]

    public class CategoriesController : Controller
    {
        private readonly test2Context _context;

        public CategoriesController(test2Context context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                // If not authenticated, challenge the user.
                await HttpContext.ChallengeAsync("default", new AuthenticationProperties());
                return new EmptyResult();
            }
            var categories = _context.Categories.Include(c => c.User);
            return View(await categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            return category == null ? NotFound() : View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            ViewBag.UserId = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, IFormFile imageFile)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Users");
            }
            var userId = int.Parse(userIdClaim.Value);

            if (!ModelState.IsValid)
            {
                ViewBag.UserId = new SelectList(_context.Users, "Id", "Username", category.UserId);
                return View(category);
            }

            if (imageFile != null && imageFile.Length > 0)
            {

                var imageUrl = await UploadImageToCloudAsync(imageFile);
                if (imageUrl == null) return RedirectToAction(nameof(Create));
                category.ImgUrl = imageUrl;
            }
            category.UserId = userId;
            _context.Entry(category).Property(b => b.UserId).IsModified = true;
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<string?> UploadImageToCloudAsync(IFormFile imageFile)
        {
            var account = new Account("dki4asjub", "286464439125792", "-i8mHPmJOri2Rnz3sue5yVktfBE");
            var cloudinary = new Cloudinary(account);

            await using var stream = imageFile.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageFile.FileName, stream),
                Folder = "categories"
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            if (uploadResult.SecureUrl != null)
                return uploadResult.SecureUrl.ToString();

            return null;

        }



        // GET: Categories/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description")] Category category)
        {
            if (id != category.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _context.Categories.FindAsync(id);
                    if (existingCategory == null)
                        return NotFound();
                    existingCategory.Title = category.Title;
                    existingCategory.Description = category.Description;

                    _context.Update(existingCategory);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(category);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();
            var category = await _context.Categories
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            return category == null ? NotFound() : View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
