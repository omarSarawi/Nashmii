using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using test2.Data;
using test2.Models;

namespace test2.Controllers
{
    [Authorize(Roles = "Admin")]

    public class HospitalController : Controller
    {
        private readonly test2Context _context;

        public HospitalController(test2Context context)
        {
            _context = context;
        }

        // GET: BloodCategories

        public async Task<IActionResult> Index()
        {
            var hospitals = _context.Hospitals
              .Include(h => h.ApprovedByUser)
              .Include(h => h.OwnerUser);
            return View(await hospitals.ToListAsync());
        }

        // GET: BloodCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hospital = await _context.Hospitals
                            .Include(h => h.ApprovedByUser)
                            .Include(h => h.OwnerUser)
                            .FirstOrDefaultAsync(h => h.Id == id);
            if (hospital == null) return NotFound();

            return View(hospital);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital == null) return NotFound();

            ViewData["ApprovedByUserId"] = new SelectList(_context.Users, "Id", "Username", hospital.ApprovedByUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Username", hospital.OwnerUserId);
            return View(hospital);
        }

        // TODO : ask the team if there is need to update the owenr and approved users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Phone,Address,ImageURL,ApprovedByUserId,OwnerUserId")] Hospital hospital)
        {
            if (id != hospital.Id)
            {
                TempData["Error"] = "Hospital ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingHospital = await _context.Hospitals.FindAsync(id);
                    if (existingHospital == null)
                    {
                        TempData["Error"] = "Hospital not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    existingHospital.Name = hospital.Name;
                    existingHospital.Email = hospital.Email;
                    existingHospital.Phone = hospital.Phone;
                    existingHospital.Address = hospital.Address;
                    existingHospital.ImageURL = hospital.ImageURL;
                    existingHospital.ApprovedByUserId = hospital.ApprovedByUserId;
                    existingHospital.OwnerUserId = hospital.OwnerUserId;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Hospital updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    TempData["Error"] = "A concurrency error occurred.";
                    TempData["ErrorDetails"] = ex.Message;
                    return RedirectToAction(nameof(Edit), new { id });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An unexpected error occurred.";
                    TempData["ErrorDetails"] = ex.Message;
                    return RedirectToAction(nameof(Edit), new { id });
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    TempData["Error"] += error.ErrorMessage + " ";
                }
            }

            // TempData["Error"] = "Validation failed. Please check the form.";
            ViewData["ApprovedByUserId"] = new SelectList(_context.Users, "Id", "Username", hospital.ApprovedByUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Username", hospital.OwnerUserId);
            return View(hospital);
        }





        // TODO: i add it but i think there is no need for it , the admin can remove just on click is better
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hospital = await _context.Hospitals
                .Include(h => h.ApprovedByUser)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hospital == null) return NotFound();

            return View(hospital);
        }

        // [HttpPost, ActionName("Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hospital = await _context.Hospitals.FindAsync(id);
            if (hospital != null)
            {
                _context.Hospitals.Remove(hospital);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HospitalExists(int id)
        {
            return _context.Hospitals.Any(e => e.Id == id);
        }
    }
}
