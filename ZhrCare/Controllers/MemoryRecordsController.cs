using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ZhrCare.Data;
using ZhrCare.Models;

namespace ZhrCare.Controllers
{
    [Authorize]
    public class MemoryRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        
        public MemoryRecordsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int? patientId)
        {
            var userId = _userManager.GetUserId(User);

            var allPatients = await _context.Patients
                .Where(p => p.CaregiverId == userId)
                .ToListAsync();
            ViewBag.AllPatients = allPatients;

            if (patientId == null)
            {
                var firstPatient = allPatients.FirstOrDefault();
                if (firstPatient == null) return View(new List<MemoryRecord>()); 
                patientId = firstPatient.Id;
            }

            var patient = allPatients.FirstOrDefault(p => p.Id == patientId);
            if (patient == null) return NotFound();

            var records = await _context.MemoryRecords
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            ViewBag.PatientName = patient.Name;
            ViewBag.PatientId = patient.Id; 

            return View(records);
        }
        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var memoryRecord = await _context.MemoryRecords
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (memoryRecord == null) return NotFound();

            return View(memoryRecord);
        }

        // GET: MemoryRecords/Create?patientId=5
        public IActionResult Create(int patientId)
        {
            ViewBag.PatientId = patientId;
            return View();
        }

        // POST: MemoryRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientId,Caption")] MemoryRecord memoryRecord, IFormFile? imageFile, IFormFile? audioFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Format not supported for images.");
                        return View(memoryRecord);
                    }
                    memoryRecord.ImagePath = await SaveFile(imageFile, "images");
                }

                if (audioFile != null)
                {
                    memoryRecord.AudioPath = await SaveFile(audioFile, "audio");
                }

                memoryRecord.CreatedAt = DateTime.Now;
                _context.Add(memoryRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { patientId = memoryRecord.PatientId });
            }
            ViewBag.PatientId = memoryRecord.PatientId;
            return View(memoryRecord);
        }

        // GET: MemoryRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var memoryRecord = await _context.MemoryRecords.FindAsync(id);
            if (memoryRecord == null) return NotFound();

            return View(memoryRecord);
        }

        // POST: MemoryRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,Caption,ImagePath,AudioPath,CreatedAt")] MemoryRecord memoryRecord, IFormFile? newImage, IFormFile? newAudio)
        {
            if (id != memoryRecord.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (newImage != null)
                    {
                        memoryRecord.ImagePath = await SaveFile(newImage, "images");
                    }

                    if (newAudio != null)
                    {
                        memoryRecord.AudioPath = await SaveFile(newAudio, "audio");
                    }

                    _context.Update(memoryRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemoryRecordExists(memoryRecord.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index), new { patientId = memoryRecord.PatientId });
            }
            return View(memoryRecord);
        }

        // GET: MemoryRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var memoryRecord = await _context.MemoryRecords
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (memoryRecord == null) return NotFound();

            return View(memoryRecord);
        }

        // POST: MemoryRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var memoryRecord = await _context.MemoryRecords.FindAsync(id);
            if (memoryRecord != null)
            {
                DeletePhysicalFile(memoryRecord.ImagePath, "images");
                DeletePhysicalFile(memoryRecord.AudioPath, "audio");

                _context.MemoryRecords.Remove(memoryRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { patientId = memoryRecord?.PatientId });
        }

        private bool MemoryRecordExists(int id)
        {
            return _context.MemoryRecords.Any(e => e.Id == id);
        }

        private async Task<string> SaveFile(IFormFile file, string subFolder)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subFolder);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fileName;
        }

        private void DeletePhysicalFile(string? fileName, string subFolder)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", subFolder, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}