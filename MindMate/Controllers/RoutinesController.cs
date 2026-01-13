using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MindMate.Data;
using MindMate.Models;

namespace MindMate.Controllers
{
    [Authorize]
    public class RoutinesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; //

        public RoutinesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        // GET: Routines
        public async Task<IActionResult> Index(int? selectedPatientId)
        {
            var userId = _userManager.GetUserId(User);
    
            ViewBag.AllPatients = await _context.Patients
                .Where(p => p.CaregiverId == userId)
                .ToListAsync();
            ViewBag.SelectedPatientId = selectedPatientId;

            var query = _context.Routines
                .Include(r => r.Patient)
                .Where(r => r.Patient.CaregiverId == userId);

            if (selectedPatientId.HasValue)
            {
                query = query.Where(r => r.PatientId == selectedPatientId.Value);
            }

            var routines = await query.ToListAsync();

            return View(routines);
        }
        
        // GET: Routines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routine = await _context.Routines
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (routine == null)
            {
                return NotFound();
            }

            return View(routine);
        }

        // GET: Routines/Create
        public async Task<IActionResult> Create()
        {
            
            var userId = _userManager.GetUserId(User); //
            
            var myPatients = await _context.Patients
                .Where(p => p.CaregiverId == userId)
                .OrderBy(p => p.Name)
                .ToListAsync(); 
            
            ViewData["PatientId"] = new SelectList(myPatients, "Id", "Name");
            return View();
 
        }
        
        

        // POST: Routines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId,ActivityName,Time,IsCompleted")] Routine routine)
        {
            var userId = _userManager.GetUserId(User); //
            
            if (ModelState.IsValid)
            {
                _context.Add(routine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PatientId"] = new SelectList(_context.Patients.Where(p => p.CaregiverId == userId), "Id", "Name", routine.PatientId);
            return View(routine);
        }

        // GET: Routines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var routine = await _context.Routines
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(m => m.Id == id && m.Patient.CaregiverId == userId);

            if (routine == null) return NotFound();

            var myPatients = _context.Patients.Where(p => p.CaregiverId == userId).ToList();
    
            ViewData["PatientId"] = new SelectList(myPatients, "Id", "Name", routine.PatientId);
            return View(routine);
        }

        // POST: Routines/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,ActivityName,Time,IsCompleted")] Routine routine)
        {
            if (id != routine.Id) return NotFound();

            var userId = _userManager.GetUserId(User);

            var isOwner = await _context.Patients
                .AnyAsync(p => p.Id == routine.PatientId && p.CaregiverId == userId);

            if (!isOwner) return Unauthorized();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(routine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoutineExists(routine.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["PatientId"] = new SelectList(_context.Patients.Where(p => p.CaregiverId == userId), "Id", "Name", routine.PatientId);
            return View(routine);
        }

        // GET: Routines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var routine = await _context.Routines
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (routine == null)
            {
                return NotFound();
            }

            return View(routine);
        }

        // POST: Routines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var routine = await _context.Routines.FindAsync(id);
            if (routine != null)
            {
                _context.Routines.Remove(routine);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoutineExists(int id)
        {
            return _context.Routines.Any(e => e.Id == id);
        }
        
        [HttpPost]
        [AllowAnonymous] // Allow patients to mark routines as 'Done' from the public dashboard without logging in
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int routineId)
        {
            var routine = await _context.Routines.FindAsync(routineId);
            if (routine == null) return NotFound();

            routine.IsCompleted = !routine.IsCompleted;
    
            _context.Update(routine);
            await _context.SaveChangesAsync();

            string returnUrl = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
        }
    }
}