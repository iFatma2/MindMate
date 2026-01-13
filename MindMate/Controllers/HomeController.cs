using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MindMate.Data;
using MindMate.Models;

namespace MindMate.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    public async Task<IActionResult> Index(int? selectedPatientId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var currentTime = DateTime.Now.TimeOfDay;
        var today = DateTime.Today;

        ViewBag.TotalPatients = await _context.Patients.CountAsync(p => p.CaregiverId == userId);
        ViewBag.TotalMemories = await _context.MemoryRecords.CountAsync(m => m.Patient.CaregiverId == userId);
        
        var allPatients = await _context.Patients
            .Where(p => p.CaregiverId == userId)
            .ToListAsync();
        ViewBag.AllPatients = allPatients;

        var currentPatient = selectedPatientId.HasValue 
            ? allPatients.FirstOrDefault(p => p.Id == selectedPatientId) 
            : allPatients.FirstOrDefault();

        if (currentPatient != null)
        {
            ViewBag.SelectedPatientId = currentPatient.Id;
            ViewBag.SelectedPatientName = currentPatient.Name;

            var medsData = await _context.Medications
                .Where(m => m.PatientId == currentPatient.Id 
                         && m.StartDate <= today 
                         && m.EndDate >= today)
                .ToListAsync();

            ViewBag.TodayMedications = medsData
                .Where(m => m.ScheduledTime.TimeOfDay >= currentTime) // تحويل الحقل لـ TimeSpan هنا
                .OrderBy(m => m.ScheduledTime)
                .Take(3)
                .ToList();

            var routinesData = await _context.Routines
                .Where(r => r.PatientId == currentPatient.Id 
                         && r.Time.Date == today)
                .ToListAsync();

            ViewBag.TodayRoutines = routinesData
                .Where(r => r.Time.TimeOfDay >= currentTime)
                .OrderBy(r => r.Time)
                .Take(3)
                .ToList();
                
            ViewBag.TodayMedsCount = ((List<Medication>)ViewBag.TodayMedications).Count;
        }
        else
        {
            ViewBag.TodayMedsCount = 0;
            ViewBag.TodayMedications = new List<Medication>();
            ViewBag.TodayRoutines = new List<Routine>();
        }

        return View(allPatients.OrderByDescending(p => p.Id).Take(3).ToList());
    }
    

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}