using GovConnect.Data;
using GovConnect.Models;
using GovConnect.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


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

        public IActionResult Status(int? grievanceId)
        {
            if (grievanceId == null)
            {
                return NotFound(); // Grievance ID not provided
            }

            // Retrieve the grievance and its timeline from the 'TimeLine' column
            var grievance = _context.DGrievances
                .Where(g => g.GrievanceID == grievanceId)
                .FirstOrDefault();

            if (grievance == null)
            {
                return NotFound(); // Grievance not found
            }

            // Deserialize the timeline from the 'TimeLine' column (assuming it's stored as a JSON string)
            var timeline = JsonConvert.DeserializeObject<List<TimeLineEntry>>(grievance.TimeLine);

            // Pass the timeline to the view
            return View(timeline);
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

            // Retrieve grievances for the logged-in user, excluding the TimeLine field
            var grievancesQuery = _context.DGrievances
                .Where(g => g.UserID == user.Id)
                .Select(g => new GrievanceViewModel
                {
                    GrievanceID = g.GrievanceID,
                    OfficerId = g.OfficerId,
                    DepartmentID = g.DepartmentID,
                    Description = g.Description,
                    FilesUploaded = g.FilesUploaded,
                    CreatedAt = g.CreatedAt,
                    UserID = g.UserID,
                    Status = g.Status,
                    Title = g.Title
                });

            // Apply status filter if provided
            if (!string.IsNullOrEmpty(statusFilter))
            {
                grievancesQuery = grievancesQuery.Where(g => g.Status == statusFilter);
            }

            // Apply ordering after filtering
            var grievances = await grievancesQuery
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            // Pass the view model list to the view
            return View(grievances);
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
