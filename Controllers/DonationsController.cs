using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using test2.Data;
using test2.Models;

namespace test2.Controllers
{
    public class DonationsController : Controller
    {
        private readonly test2Context _context;

        public DonationsController(test2Context context)
        {
            _context = context;
        }

        // GET: Donations
        public async Task<IActionResult> Index()
        {
            var test2Context = _context.Donations.Include(d => d.Comping).Include(d => d.User);
            return View(await test2Context.ToListAsync());
        }

        // GET: Donations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations
                .Include(d => d.Comping)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // GET: Donations/Create
        public IActionResult Create()
        {
            ViewData["CompingId"] = new SelectList(_context.Compings, "Id", "Title");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Donations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Donation donation)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CompingId = new SelectList(_context.Compings, "Id", "Title", donation.CompingId);
                ViewBag.UserId = new SelectList(_context.Users, "Id", "Username", donation.UserId);
                return View(donation);
            }

            _context.Add(donation);
            var comping = await _context.Compings.FindAsync(donation.CompingId);
            if (comping != null)
            {
                comping.Amount += (int?)donation.Amount;
                _context.Entry(comping).Property(c => c.Amount).IsModified = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }


        // GET: Donations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
            {
                return NotFound();
            }
            ViewData["CompingId"] = new SelectList(_context.Compings, "Id", "Title", donation.CompingId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", donation.UserId);
            return View(donation);
        }

        // POST: Donations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompingId,UserId,Amount")] Donation donation)
        {
            if (id != donation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ابحث عن الـ Comping حسب CompingId
                    var comping = await _context.Compings.FindAsync(donation.CompingId);
                    if (comping != null)
                    {
                        // إضافة قيمة التبرع إلى الـ Amount الحالي مع تحويلها إلى double
                        comping.Amount += (int?)donation.Amount;

                        // تحديث قيمة الـ Comping
                        _context.Entry(comping).Property(c => c.Amount).IsModified = true;
                    }

                    // تحديث التبرع
                    _context.Update(donation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonationExists(donation.Id))
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

            ViewData["CompingId"] = new SelectList(_context.Compings, "Id", "Title", donation.CompingId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", donation.UserId);
            return View(donation);
        }

        // GET: Donations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations
                .Include(d => d.Comping)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // POST: Donations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation != null)
            {
                _context.Donations.Remove(donation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DonationExists(int id)
        {
            return _context.Donations.Any(e => e.Id == id);
        }
    }
}
