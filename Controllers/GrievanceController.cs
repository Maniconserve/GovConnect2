namespace GovConnect.Controllers
{
    public class GrievanceController : Controller
    {
        private readonly IGrievanceService _grievanceService;
        private readonly UserManager<Citizen> _citizenManager;

        public GrievanceController(IGrievanceService grievanceService, UserManager<Citizen> citizenManager)
        {
            _grievanceService = grievanceService;
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
                var user = await _citizenManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login","Citizen");
                }

                bool success = await _grievanceService.LodgeGrievanceAsync(grievance, user.Id, fileUpload);

                if (success)
                {
                    TempData["GrievanceID"] = grievance.GrievanceID;
                    return RedirectToAction("Lodge");
                }
                ModelState.AddModelError(string.Empty, "Error lodging grievance.");
            }

            return View(grievance);
        }
        [HttpGet]
        public IActionResult Status()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Status(int? grievanceId)
        {
            if (!grievanceId.HasValue)
            {
                return NotFound();
            }

            var grievance = _grievanceService.GetGrievanceByIdAsync(grievanceId.Value).Result;

            if (grievance == null)
            {
                return View();
            }

            var timeline = _grievanceService.GetGrievanceTimelineAsync(grievanceId.Value).Result;

            return View(timeline);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyGrievances(string? statusFilter)
        {
            var user = await _citizenManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Citizen");
            }

            var grievances = await _grievanceService.GetGrievancesByUserAsync(user.Id, statusFilter);
            return View(grievances);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var grievance = await _grievanceService.GetGrievanceByIdAsync(id);
            if (grievance == null)
            {
                return NotFound();
            }
            return View(grievance);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(int id, IFormFile fileUpload)
        {
            var success = await _grievanceService.UpdateGrievanceFilesAsync(id, fileUpload);
            if (success)
            {
                return RedirectToAction("Details", new { id });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> EscalateGrievance(int grievanceId,int OfficerId)
        {
            var result = await _grievanceService.EscalateGrievanceAsync(grievanceId);

            if (result)
            {
                TempData["EscalateMessage"] = "Grievance successfully escalated!";
                return RedirectToAction("Dashboard", "Officer", new { officerId = OfficerId });
            }

            TempData["EscalateMessage"] = "No superior officer found for the department!";
            return RedirectToAction("Dashboard","Officer", new { officerId = OfficerId });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var fileData = await _grievanceService.GetGrievanceFileAsync(id);

            if (fileData == null)
            {
                return NotFound(); 
            }
            string fileName = "GrievanceFile.pdf"; 
            string mimeType = "application/pdf";   

            return File(fileData, mimeType, fileName);
        }

    }
}
