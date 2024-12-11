using System.Text.Json;

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

        /// <summary>
        /// Displays the default grievance dashboard (index page).
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the form to lodge a new grievance.
        /// </summary>
        [HttpGet]
        public IActionResult Lodge()
        {
            return View();
        }

        /// <summary>
        /// Handles the POST request to lodge a new grievance, including optional file upload.
        /// </summary>
        /// <param name="grievance">The grievance data submitted by the user.</param>
        /// <param name="fileUpload">An optional file to attach to the grievance.</param>
        [HttpPost]
        public async Task<IActionResult> Lodge(Grievance grievance, List<IFormFile?> files)
        {
            if (ModelState.IsValid)
            {
                var user = await _citizenManager.GetUserAsync(User); // Get the current logged-in user
                if (user == null)
                {
                    return RedirectToAction("Login", "Citizen"); // Redirect to login if no user is authenticated
                }

                // Call the service to lodge the grievance
                bool success = await _grievanceService.LodgeGrievanceAsync(grievance, user.Id, files);

                if (success)
                {
                    TempData["GrievanceID"] = grievance.GrievanceID; // Store the grievance ID in TempData for success
                    return RedirectToAction("Lodge"); // Redirect back to the lodge page
                }

                ModelState.AddModelError(string.Empty, "Error lodging grievance."); // Display error if grievance couldn't be lodged
            }

            return View(grievance); // Return the form with validation errors
        }

        /// <summary>
        /// Displays the grievance status page where the user can check the status of a grievance.
        /// </summary>
        [HttpGet]
        public IActionResult Status()
        {
            return View();
        }

        /// <summary>
        /// Handles the POST request to view the status of a grievance by its ID.
        /// </summary>
        /// <param name="grievanceId">The ID of the grievance whose status is being checked.</param>
        [HttpPost]
        public IActionResult Status(int? grievanceId)
        {
            if (!grievanceId.HasValue)
            {
                return NotFound(); // Return 404 if no grievance ID is provided
            }

            var grievance = _grievanceService.GetGrievanceByIdAsync(grievanceId.Value).Result; // Get grievance details
            if (grievance == null)
            {
                return View(); // Return an empty view if grievance not found
            }

            var timeline = _grievanceService.GetGrievanceTimelineAsync(grievanceId.Value).Result; // Get the timeline of actions for the grievance

            return View(timeline); // Return the timeline view
        }

        /// <summary>
        /// Displays a list of grievances for the logged-in user with optional filtering by status.
        /// </summary>
        /// <param name="statusFilter">Optional status filter for grievances.</param>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyGrievances(Status? statusFilter)
        {
            var user = await _citizenManager.GetUserAsync(User); // Get the current logged-in user
            if (user == null)
            {
                return RedirectToAction("Login", "Citizen"); // Redirect to login if no user is authenticated
            }

            var grievances = await _grievanceService.GetGrievancesByUserAsync(user.Id, statusFilter); // Get grievances by user and status filter
            ViewBag.Status = statusFilter;
            return View(grievances); // Return the grievances to the view
        }

        /// <summary>
        /// Displays the details of a specific grievance.
        /// </summary>
        /// <param name="id">The ID of the grievance whose details are being viewed.</param>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var grievance = await _grievanceService.GetGrievanceByIdAsync(id); // Get the grievance details
            if (grievance == null)
            {
                return NotFound(); // Return 404 if grievance not found
            }
            var files = await _grievanceService.GetGrievanceFileAsync(id);
            var viewModel = new GrievanceDetailsViewModel
            {
                Grievance = grievance,
                Files = files
            };

            return View(viewModel);
        }

        /// <summary>
        /// Handles the upload of files to a specific grievance.
        /// </summary>
        /// <param name="id">The ID of the grievance for which the file is being uploaded.</param>
        /// <param name="files">The files to be uploaded.</param>
        [HttpPost]
        public async Task<IActionResult> UploadFile(int id, List<IFormFile> files)
        {
            var success = await _grievanceService.UpdateGrievanceFilesAsync(id, files); // Upload the file to the grievance
            if (success)
            {
                return RedirectToAction("Details", new { id }); // Redirect back to the grievance details page
            }

            return NotFound(); // Return 404 if file upload fails
        }

        /// <summary>
        /// Handles the escalation of a grievance to a superior officer.
        /// </summary>
        /// <param name="grievanceId">The ID of the grievance to escalate.</param>
        /// <param name="OfficerId">The ID of the officer to whom the grievance is being escalated.</param>
        [HttpPost]
        public async Task<IActionResult> EscalateGrievance(int grievanceId, int OfficerId)
        {
            var result = await _grievanceService.EscalateGrievanceAsync(grievanceId); // Escalate the grievance

            if (result)
            {
                TempData["EscalateMessage"] = "Grievance successfully escalated!"; // Success message
                return RedirectToAction("Dashboard", "Officer", new { officerId = OfficerId }); // Redirect to officer dashboard
            }

            TempData["EscalateMessage"] = "No superior officer found for the department!"; // Error message
            return RedirectToAction("Dashboard", "Officer", new { officerId = OfficerId }); // Redirect to officer dashboard
        }

        /// <summary>
        /// Adds a new timeline entry to a specific grievance, indicating progress.
        /// </summary>
        /// <param name="grievanceId">The ID of the grievance to which the timeline entry is being added.</param>
        /// <param name="date">The date of the work entry.</param>
        /// <param name="work">A description of the work done.</param>
        [HttpPost]
        public async Task<IActionResult> AddTimeLineEntry(int grievanceId, DateTime date, string work)
        {
            var grievance = await _grievanceService.GetGrievanceByIdAsync(grievanceId); // Get the grievance details

            if (grievance != null)
            {
                var timeLine = string.IsNullOrEmpty(grievance.TimeLine)
                    ? new List<TimeLineEntry>() // If no timeline exists, create a new list
                    : JsonSerializer.Deserialize<List<TimeLineEntry>>(grievance.TimeLine); // Deserialize the existing timeline

                timeLine.Add(new TimeLineEntry
                {
                    Date = date,
                    Work = work
                });

                grievance.Status = Models.Status.InProgress; // Set grievance status to 'In Progress'
                grievance.SetTimeLine(timeLine); // Update the timeline

                await _grievanceService.UpdateAsync(grievance); // Update the grievance in the database
            }

            return RedirectToAction("Details", new { id = grievanceId }); // Redirect to grievance details page
        }

        /// <summary>
        /// Allows the user to download a file associated with a specific grievance.
        /// </summary>
        /// <param name="id">The ID of the grievance for which the file is being downloaded.</param>
        [HttpGet]
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            var fileData = await _grievanceService.GetFileAsync(fileId); // Get the file data for the grievance

            if (fileData == null)
            {
                return NotFound(); // Return 404 if no file is found
            }

            // Get the file's content and other properties
            byte[] fileBytes = fileData.FileContent; // The actual file content as a byte array
            string fileName = fileData.FileName ?? "GrievanceFile.pdf"; // Use file name from the database or default to "GrievanceFile.pdf"
            string mimeType = "application/pdf"; // Default MIME type for PDF (you can adjust this based on your file types)

            return File(fileBytes, mimeType, fileName); // Return the file for download
        }
        [HttpGet]
        public async Task<IActionResult> ViewFile(int fileId)
        {
            var file = await _grievanceService.GetFileAsync(fileId); // Retrieve the file using the fileId
            if (file == null)
            {
                return NotFound(); // Return 404 if the file is not found
            }
            string fileName = file.FileName;
            string[] parts = fileName.Split('.');
            string fileExtension = parts.Last().ToLower();  // Extract file extension

            // Determine MIME type based on the file extension
            string mimeType;
            switch (fileExtension)
            {
                case "pdf":
                    mimeType = "application/pdf";
                    break;
                case "jpg":
                case "jpeg":
                    mimeType = "image/jpeg";
                    break;
                case "png":
                    mimeType = "image/png";
                    break;
                case "txt":
                    mimeType = "text/plain";
                    break;
                case "html":
                    mimeType = "text/html";
                    break;
                case "docx":
                    mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case "xlsx":
                    mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                default:
                    mimeType = "application/octet-stream";  // Default binary stream if unknown extension
                    break;
            }
            return File(file.FileContent, mimeType); // You can change the MIME type based on the file type
        }


    }
}
