// using System.Diagnostics;
// using Microsoft.AspNetCore.Mvc;
// using ZhrCare.Models;
//
// namespace ZhrCare.Controllers;
//
// public class HomeController : Controller
// {
//     public IActionResult Index()
//     {
//         return View();
//     }
//
//     public IActionResult Privacy()
//     {
//         return View();
//     }
//
//     [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//     public IActionResult Error()
//     {
//         return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//     }
// }


using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZhrCare.Models;
using ZhrCare.Data;
using Microsoft.AspNetCore.Identity;

namespace ZhrCare.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // public async Task<IActionResult> Index()
    // {
    //     var userId = _userManager.GetUserId(User);
    //     if (userId == null)
    //     {
    //         return RedirectToPage("/Account/Login", new { area = "Identity" });
    //     }
    //
    //     // 1. Statistics (KPIs)
    //     ViewBag.TotalPatients = await _context.Patients.CountAsync(p => p.CaregiverId == userId);
    //     ViewBag.TodayMedsCount = await _context.Medications.CountAsync(m => m.Patient.CaregiverId == userId);
    //     ViewBag.TotalMemories = await _context.MemoryRecords.CountAsync(m => m.Patient.CaregiverId == userId);
    //
    //     // 2. Recently Added Patients
    //     var recentPatients = await _context.Patients
    //         .Where(p => p.CaregiverId == userId)
    //         .OrderByDescending(p => p.Id)
    //         .Take(3)
    //         .ToListAsync();
    //
    //     // 3. Upcoming Medications for Today
    //     ViewBag.TodayMedications = await _context.Medications
    //         .Include(m => m.Patient)
    //         .Where(m => m.Patient.CaregiverId == userId)
    //         .OrderBy(m => m.ScheduledTime)
    //         .Take(4)
    //         .ToListAsync();
    //
    //     // 4. Daily Routines for Today
    //     ViewBag.TodayRoutines = await _context.Routines
    //         .Include(r => r.Patient)
    //         .Where(r => r.Patient.CaregiverId == userId)
    //         .OrderBy(r => r.Time)
    //         .Take(4)
    //         .ToListAsync();
    //
    //     return View(recentPatients);
    // }
    public async Task<IActionResult> Index(int? selectedPatientId)
{
    var userId = _userManager.GetUserId(User);
    if (userId == null)
    {
        return RedirectToPage("/Account/Login", new { area = "Identity" });
    }

    // 1. General Statistics (Always shown)
    ViewBag.TotalPatients = await _context.Patients.CountAsync(p => p.CaregiverId == userId);
    ViewBag.TotalMemories = await _context.MemoryRecords.CountAsync(m => m.Patient.CaregiverId == userId);
    
    // 2. Get all patients for the dropdown selector
    var allPatients = await _context.Patients
        .Where(p => p.CaregiverId == userId)
        .ToListAsync();
    ViewBag.AllPatients = allPatients;

    // 3. Determine which patient to display (the selected one or the first one found)
    var currentPatient = selectedPatientId.HasValue 
        ? allPatients.FirstOrDefault(p => p.Id == selectedPatientId) 
        : allPatients.FirstOrDefault();

    if (currentPatient != null)
    {
        ViewBag.SelectedPatientId = currentPatient.Id;
        ViewBag.SelectedPatientName = currentPatient.Name;

        // 4. Specific data for the SELECTED patient only
        ViewBag.TodayMedications = await _context.Medications
            .Where(m => m.PatientId == currentPatient.Id)
            .OrderBy(m => m.ScheduledTime)
            .ToListAsync();

        ViewBag.TodayRoutines = await _context.Routines
            .Where(r => r.PatientId == currentPatient.Id)
            .OrderBy(r => r.Time)
            .ToListAsync();
            
        // Get the medication count for this specific patient for the KPI card
        ViewBag.TodayMedsCount = ViewBag.TodayMedications.Count;
    }
    else
    {
        // Fallback if no patients exist yet
        ViewBag.TodayMedsCount = 0;
        ViewBag.TodayMedications = new List<Medication>();
        ViewBag.TodayRoutines = new List<Routine>();
    }

    // Return the list of patients to the view as the model (for the 'Recent Patients' list)
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