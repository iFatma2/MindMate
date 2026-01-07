using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZhrCare.Data;
using ZhrCare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ZhrCare.Controllers
{
    [Authorize]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; //

        public PatientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) //
        {
            _context = context;
            _userManager = userManager; //
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {

            var userId = _userManager.GetUserId(User); 
            var today = DateTime.Today;
            var dayName = today.DayOfWeek.ToString();
                
            var patients = await _context.Patients
                .Where(p => p.CaregiverId == userId)
                .Include(p => p.Medications)
                .ThenInclude(m => m.MedicationLogs.Where(l => l.TakenDate.Date == today))
                .ToListAsync();
            
            
            return View(patients);
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.Caregiver)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            ViewData["CaregiverId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Age")] Patient patient)
        {
            ModelState.Remove("CaregiverId");
            ModelState.Remove("AccessToken");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Caregiver");
            
            if (ModelState.IsValid)
            {
                patient.CaregiverId = _userManager.GetUserId(User);//
                patient.CreatedAt = DateTime.Now;
                patient.AccessToken = Guid.NewGuid();
            
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            ViewData["CaregiverId"] = new SelectList(_context.Users, "Id", "Id", patient.CaregiverId);
            return View(patient);
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Age,AccessToken,CreatedAt,CaregiverId")] Patient patient)
        {
            if (id != patient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
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
            ViewData["CaregiverId"] = new SelectList(_context.Users, "Id", "Id", patient.CaregiverId);
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.Caregiver)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
        
        // Inside PatientsController.cs

        public async Task<IActionResult> Schedule(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .Include(p => p.Medications)
                .ThenInclude(m => m.MedicationLogs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null) return NotFound();

            var today = DateTime.Today;
            var dayOfWeek = today.DayOfWeek.ToString();

            // Logic to filter medications for "Today"
            var todayMedications = patient.Medications.Where(m =>
                today >= m.StartDate && today <= m.EndDate &&
                (m.FrequencyType == "Daily" || 
                 (m.FrequencyType == "Weekly" && m.SelectedDays != null && m.SelectedDays.Contains(dayOfWeek)))
            ).ToList();

            ViewBag.PatientName = patient.Name;
            ViewBag.PatientId = patient.Id;

            return View(todayMedications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsTaken(int medicationId, int patientId)
        {
            var log = new MedicationLog
            {
                MedicationId = medicationId,
                TakenDate = DateTime.Today,
                TakenTime = DateTime.Now,
                Status = "Taken"
            };

            _context.MedicationLogs.Add(log);
            await _context.SaveChangesAsync();

            // Redirect back to the patient's schedule
            return RedirectToAction(nameof(Schedule), new { id = patientId });
        }
    }
}
