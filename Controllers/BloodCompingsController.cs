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
    public class BloodCompingsController : Controller
    {
        private readonly test2Context _context;

        public BloodCompingsController(test2Context context)
        {
            _context = context;
        }

        // GET: BloodCompings
        public async Task<IActionResult> Index()
        {
            var test2Context = _context.BloodCompings.Include(b => b.Hospital).Include(b => b.User);
            return View(await test2Context.ToListAsync());
        }

        // GET: BloodCompings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloodComping = await _context.BloodCompings
                .Include(b => b.Hospital)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bloodComping == null)
            {
                return NotFound();
            }

            return View(bloodComping);
        }

        // GET: BloodCompings/Create
        public IActionResult Create()
        {
            ViewData["BloodCategoryId"] = new SelectList(_context.Hospitals, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: BloodCompings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BloodComping bloodComping)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var userId = int.Parse(userIdClaim.Value);

            var hospital = await _context.Hospitals.FirstOrDefaultAsync(h => h.OwnerUserId == userId);
            if (hospital == null)
            {
                ModelState.AddModelError("", "No hospital found for this user.");
                return View(bloodComping);
            }

            bloodComping.UserId = userId;
            bloodComping.HospitalId = hospital.Id;

            if (ModelState.IsValid)
            {
                _context.Add(bloodComping);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "HospitalDashboard");
            
            }

            return View(bloodComping);
        }


        // GET: BloodCompings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloodComping = await _context.BloodCompings.FindAsync(id);
            if (bloodComping == null)
            {
                return NotFound();
            }
            ViewData["BloodCategoryId"] = new SelectList(_context.Hospitals, "Id", "Name", bloodComping.HospitalId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", bloodComping.UserId);
            return View(bloodComping);
        }

        // POST: BloodCompings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Type,Amount,Goal,BloodCategoryId,UserId")] BloodComping bloodComping)
        {
            if (id != bloodComping.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bloodComping);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BloodCompingExists(bloodComping.Id))
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
            ViewData["BloodCategoryId"] = new SelectList(_context.Hospitals, "Id", "Name", bloodComping.HospitalId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", bloodComping.UserId);
            return View(bloodComping);
        }

        // GET: BloodCompings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bloodComping = await _context.BloodCompings
                .Include(b => b.Hospital)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bloodComping == null)
            {
                return NotFound();
            }

            return View(bloodComping);
        }

        // POST: BloodCompings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bloodComping = await _context.BloodCompings.FindAsync(id);
            if (bloodComping != null)
            {
                _context.BloodCompings.Remove(bloodComping);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BloodCompingExists(int id)
        {
            return _context.BloodCompings.Any(e => e.Id == id);
        }
    }
}
