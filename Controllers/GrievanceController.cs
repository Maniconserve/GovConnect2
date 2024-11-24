using GovConnect.Data;
using GovConnect.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovConnect.Controllers
{
    public class GrievanceController : Controller
    {
        private readonly SqlServerDbContext _context;
        private readonly UserManager<Citizen> _citizenManager;
        public GrievanceController(SqlServerDbContext context, UserManager<Citizen> citizenManager)
        {
            _context = context;
            _citizenManager = citizenManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Lodge()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Lodge(Grievance grievance, IFormFile? fileUpload)
        {
            if (ModelState.IsValid)
            {
                var random = new Random();
                int randomGrievanceId;
                do
                {
                    randomGrievanceId = random.Next(100000, 999999); // Generates a 6-digit number
                } while (_context.DGrievances.Any(g => g.GrievanceID == randomGrievanceId));
                grievance.GrievanceID = randomGrievanceId;
                if (fileUpload != null && fileUpload.Length > 0)
                {
                    // Convert the file to a byte array
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileUpload.CopyToAsync(memoryStream);
                        grievance.FilesUploaded = memoryStream.ToArray();
                    }
                }
                // Save the grievance to the database (example)
                // Assuming you have a DbContext and a Grievances DbSet
                var user = await _citizenManager.GetUserAsync(User);
                grievance.UserID = user.Id;
                _context.DGrievances.Add(grievance);
                _context.SaveChanges();

                TempData["GrievanceID"] = randomGrievanceId;

                // Redirect back to the form or confirmation page
                return RedirectToAction("Lodge");
            }

            // If the model is not valid, redisplay the form with error messages
            return View(grievance);
        }

        [HttpGet]
        public IActionResult Status()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> MyGrievances(string? statusFilter)
        {
            // Get the currently logged-in user
            var user = await _citizenManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if user is not authenticated
            }

            // Retrieve grievances for the logged-in user
            var grievancesQuery = _context.DGrievances
                .Where(g => g.UserID == user.Id);

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(statusFilter))
            {
                grievancesQuery = grievancesQuery.Where(g => g.Status == statusFilter);
            }

            // Apply ordering after filtering
            var grievances = await grievancesQuery
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return View(grievances); // Pass filtered grievances to the view
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Retrieve the grievance from the database
            var grievance = await _context.DGrievances.FirstOrDefaultAsync(g => g.GrievanceID == id);

            if (grievance == null)
            {
                return NotFound(); // Return a 404 if the grievance is not found
            }

            return View(grievance);
        }
        public async Task<IActionResult> DownloadFile(int id)
        {
            var grievance = await _context.DGrievances.FirstOrDefaultAsync(g => g.GrievanceID == id);

            if (grievance == null || grievance.FilesUploaded == null)
            {
                return NotFound();
            }

            // Return the file for download
            return File(grievance.FilesUploaded, "application/octet-stream", "GrievanceFile");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(int id, IFormFile fileUpload)
        {
            var grievance = await _context.DGrievances.FirstOrDefaultAsync(g => g.GrievanceID == id);

            if (grievance == null)
            {
                return NotFound();
            }

            if (fileUpload != null && fileUpload.Length > 0)
            {
                // Convert the uploaded file to a byte array
                using (var memoryStream = new MemoryStream())
                {
                    await fileUpload.CopyToAsync(memoryStream);
                    grievance.FilesUploaded = memoryStream.ToArray();
                }

                // Save the file to the database
                _context.Update(grievance);
                await _context.SaveChangesAsync();
            }

            // Redirect back to the details view with the updated grievance
            return RedirectToAction("Details", new { id = grievance.GrievanceID });
        }

    }
}
