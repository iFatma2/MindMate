using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; //
using Microsoft.AspNetCore.Authorization;
using MindMate.Data;
using MindMate.Models; //

namespace MindMate.Controllers
{
    [Authorize] //
    public class MedicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; //

        public MedicationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) //
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? selectedPatientId)
        {
            var userId = _userManager.GetUserId(User);
    
            ViewBag.AllPatients = await _context.Patients
                .Where(p => p.CaregiverId == userId)
                .ToListAsync();
        
            ViewBag.SelectedPatientId = selectedPatientId;

            var query = _context.Medications
                .Include(m => m.Patient)
                .Where(m => m.Patient.CaregiverId == userId);

            if (selectedPatientId.HasValue)
            {
                query = query.Where(m => m.PatientId == selectedPatientId);
            }

            var medications = await query.ToListAsync();

            return View(medications);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        // GET: Medications/Create
        public IActionResult Create()
        {
            //
            var userId = _userManager.GetUserId(User);

            var myPatients = _context.Patients
                .Where(p => p.CaregiverId == userId)
                .ToList();

            ViewData["PatientId"] = new SelectList(myPatients, "Id", "Name");
            return View();
        }

        // POST: Medications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        //
        public async Task<IActionResult> Create([Bind("PatientId,Name,Dosage,FrequencyType,SelectedDays,ScheduledTime,StartDate,EndDate")] Medication medication)
        {
            var userId = _userManager.GetUserId(User);
            var isOwner = await _context.Patients.AnyAsync(p => p.Id == medication.PatientId && p.CaregiverId == userId);
            if (!isOwner) return Unauthorized();

            if (ModelState.IsValid)
            {
                _context.Add(medication);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PatientId"] = new SelectList(_context.Patients.Where(p => p.CaregiverId == userId), "Id", "Name", medication.PatientId);
            return View(medication);
        }

        // GET: Medications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = _userManager.GetUserId(User);
            var medication = await _context.Medications
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id && m.Patient.CaregiverId == userId);
            
            if (id == null)
            {
                return NotFound();
            }
            
            var myPatients = _context.Patients.Where(p => p.CaregiverId == userId).ToList();
            ViewData["PatientId"] = new SelectList(myPatients, "Id", "Name");
            
            return View(medication);
        }

        // POST: Medications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,Name,Dosage,FrequencyType,SelectedDays,ScheduledTime,StartDate,EndDate")] Medication medication)
        {
            if (id != medication.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = _userManager.GetUserId(User);
                    var isOwner = await _context.Patients.AnyAsync(p => p.Id == medication.PatientId && p.CaregiverId == userId);
                    if (!isOwner) return Unauthorized();
                    
                    _context.Update(medication);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicationExists(medication.Id))
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
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", medication.PatientId);
            return View(medication);
        }

        // GET: Medications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        // POST: Medications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication != null)
            {
                _context.Medications.Remove(medication);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MedicationExists(int id)
        {
            return _context.Medications.Any(e => e.Id == id);
        }
    }
}
