using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using test2.Data;
using test2.Models;

namespace test2.Controllers
{
    public class CompingsController : Controller
    {
        private readonly test2Context _context;

        public CompingsController(test2Context context)
        {
            _context = context;
        }

        // GET: Compings
        public async Task<IActionResult> Index()
        {
            var test2Context = _context.Compings.Include(c => c.Category).Include(c => c.User);
            return View(await test2Context.ToListAsync());
        }

        // GET: Compings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comping = await _context.Compings
                .Include(c => c.Category)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comping == null)
            {
                return NotFound();
            }

            return View(comping);
        }

        public IActionResult Create()
        {
            ViewData["CategoryTitle"] = new SelectList(_context.Categories, "Title", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompingViewModel vm, IFormFile imageFile)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Users");
            }
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Title == vm.CategoryTitle);
            if (category == null)
            {
                ModelState.AddModelError("CategoryTitle", "Invalid category title.");
                ViewBag.CategoryTitle = new SelectList(_context.Categories, "Title", "Title", vm.CategoryTitle);
                return View(vm);
            }
            var comping = new Comping
            {
                Title = vm.Title,
                ShortDesc = vm.ShortDesc,
                LongDesc = vm.LongDesc,
                Goal = vm.Goal,
                Amount = vm.Amount,
                CategoryId = category.Id,
                UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
            };

            if (!ModelState.IsValid)
            {
                ViewBag.CategoryTitle = new SelectList(_context.Categories, "Id", "Title", comping.CategoryId);
                return View(comping);
            }
            if (imageFile != null && imageFile.Length > 0)
            {
                var imageUploadService = new ImageUploadService();
                var imageUrl = await imageUploadService.UploadImageAsync(imageFile);
                if (imageUrl == null) return RedirectToAction(nameof(Create));
                comping.ImgUrl = imageUrl;
            }

            _context.Add(comping);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var comping = await _context.Compings.FindAsync(id);
            if (comping == null)
                return NotFound();

            ViewBag.UserId = new SelectList(_context.Users, "Id", "Username", comping.UserId);
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Title", comping.CategoryId);
            return View(comping);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Comping comping, IFormFile imageFile)
        {
            if (id != comping.Id)
                return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                var imageUploadService = new ImageUploadService();
                var imageUrl = await imageUploadService.UploadImageAsync(imageFile);

                if (imageUrl != null)
                {
                    comping.ImgUrl = imageUrl;

                    ModelState.Remove("ImgUrl");
                }
            }
            else
            {
                ModelState.Remove("ImgUrl");
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["Error"] = string.Join(" | ", errorMessages);

                ViewBag.UserId = new SelectList(_context.Users, "Id", "Username", comping.UserId);
                ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Title", comping.CategoryId);
                return View(comping);
            }

            var compingInDb = await _context.Compings.FindAsync(id);
            if (compingInDb == null)
                return NotFound();

            try
            {
                compingInDb.Title = comping.Title;
                compingInDb.ShortDesc = comping.ShortDesc;
                compingInDb.LongDesc = comping.LongDesc;
                compingInDb.Goal = comping.Goal;
                compingInDb.Amount = comping.Amount;
                compingInDb.CategoryId = comping.CategoryId;
                compingInDb.UserId = comping.UserId;

                // Only update image if a new one was uploaded
                if (comping.ImgUrl != null)
                {
                    compingInDb.ImgUrl = comping.ImgUrl;
                }

                _context.Update(compingInDb);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Comping updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Compings.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
        }

        // GET: Compings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comping = await _context.Compings
                .Include(c => c.Category)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comping == null)
            {
                return NotFound();
            }

            return View(comping);
        }

        // POST: Compings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comping = await _context.Compings.FindAsync(id);
            if (comping != null)
            {
                _context.Compings.Remove(comping);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompingExists(int id)
        {
            return _context.Compings.Any(e => e.Id == id);
        }
    }
}
