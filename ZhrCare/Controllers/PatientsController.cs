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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Age")] Patient patient)
        {
            if (id != patient.Id) return NotFound();

            ModelState.Remove("CaregiverId");
            ModelState.Remove("AccessToken");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Caregiver"); // إذا كان هناك Navigation Property

            if (ModelState.IsValid)
            {
                try
                {
                    var patientToUpdate = await _context.Patients.FindAsync(id);
                    if (patientToUpdate == null) return NotFound();

                    patientToUpdate.Name = patient.Name;
                    patientToUpdate.Age = patient.Age;

                    _context.Update(patientToUpdate);
                    await _context.SaveChangesAsync();
            
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id)) return NotFound();
                    else throw;
                }
            }
    
            return View(patient);
        }
        
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
            var today = DateTime.Today;

            var existingLog = await _context.MedicationLogs
                .FirstOrDefaultAsync(l => l.MedicationId == medicationId && l.TakenDate.Date == today);

            if (existingLog != null)
            {
                _context.MedicationLogs.Remove(existingLog);
            }
            else
            {
                var log = new MedicationLog
                {
                    MedicationId = medicationId,
                    TakenDate = today,
                    TakenTime = DateTime.Now,
                    Status = "Taken"
                };
                _context.MedicationLogs.Add(log);
            }

            await _context.SaveChangesAsync(); 
            
            string returnUrl = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        
        public async Task<IActionResult> PatientDashboard(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Medications)
                .ThenInclude(m => m.MedicationLogs)
                .Include(p => p.Routines)
                .Include(p => p.MemoryRecords) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patient == null) return NotFound();

            var viewModel = new PatientDashboardViewModel
            {
                PatientId = patient.Id,
                PatientName = patient.Name,
                Medications = patient.Medications.ToList(),
                Routines = patient.Routines.ToList(),
                Memories = patient.MemoryRecords.ToList() 
            };

            return View(viewModel);
        }
    }
}
