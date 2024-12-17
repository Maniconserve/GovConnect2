using System.Security.Claims;

namespace GovConnect.Controllers
{
    public class OfficerController : Controller
    {
        private ICitizenService _citizenService;
        private IGrievanceService _grievanceService;
        private IOfficerService _officerService;
        private DashboardService _dashboardService;

        public OfficerController(ICitizenService citizenService, IGrievanceService grievanceService, IOfficerService officerService, UserManager<Citizen> officerManager,SignInManager<Citizen> signInManager,DashboardService dashboardService)
        {
            _citizenService = citizenService;
            _grievanceService = grievanceService;
            _officerService = officerService;
            _dashboardService = dashboardService;
        }

        ///// <summary>
        ///// Displays the login page if the officer is not authenticated.
        ///// Redirects to the Scheme index page if the officer is already logged in.
        ///// </summary>

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        ///// <summary>
        ///// Handles the POST request to log in the officer.
        ///// Validates officer credentials and sets authentication cookies on success.
        ///// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Find the user by their email.
                var user = await _citizenService.GetUserByEmailAsync(model.Email);

                if (user != null)
                {
                    // Check if the user is in the "Officer" role before attempting to sign in
                    var isOfficer = await _citizenService.CheckRoleAsync(user, "Officer");

                    if (isOfficer)
                    {
                        HttpContext.Session.SetString("officerId", user.Id);
                        // Perform password-based sign-in only if the user is an officer
                        var result = await _citizenService.SignInUserAsync(user, model, false, lockoutOnFailure: false);

                        if (result.Succeeded)
                        {
                            // If login is successful, redirect to the requested return URL or the Scheme Index page.
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            {
                                return Redirect(returnUrl);
                            }
                            else
                            {
                                return RedirectToAction("Dashboard", new { officerId = user.Id });
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            return View(model);
                        }
                    }
                    else
                    {
                        // If the user is not in the "Officer" role, add an error to the model.
                        ModelState.AddModelError(string.Empty, "You are not an officer");
                        return View(model);
                    }
                }
                else
                {
                    // If the user doesn't exist, add an error to the model state.
                    ModelState.AddModelError(string.Empty, "Officer doesn't exist.");
                    return View(model);
                }
            }

            // If the model is invalid, return the same view with validation errors
            return View(model);
        }



        ///// <summary>
        ///// Displays the officer dashboard for a specific officer.
        ///// </summary>
        ///// <param name="officerId">The officer's ID</param>
        [Authorize(Roles = "Officer")]
        [HttpGet]
        public async Task<IActionResult> Dashboard(string officerId)
        {
            var model = await _dashboardService.GetOfficerDashboard(officerId);
            var officer = await _citizenService.GetUserAsync(User);
            model.OfficerName = officer.UserName;
            if (model == null)
            {
                return NotFound(); // Return 404 if no dashboard data is found
            }

            return View(model); // Return the dashboard view
        }

        ///// <summary>
        ///// Displays the details of a specific grievance.
        ///// </summary>
        ///// <param name="id">The grievance ID</param>
        [Authorize(Roles = "Officer")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var grievance = await _grievanceService.GetGrievanceByIdAsync(id);
            if(grievance.OfficerId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return View("AccessDenied");
            }
            if (grievance == null)
            {
                return NotFound(); 
            }
            var files = await _grievanceService.GetGrievanceFileAsync(id);
            var viewModel = new GrievanceDetailsViewModel
            {
                Grievance = grievance,
                Files = files
            };
            return View(viewModel); // Return the grievance details view
        }

		[HttpPost]
		public async Task<IActionResult> UpdateStatus(int complaintId, Status status)
		{
			try
			{
				// Fetch the complaint based on the provided complaintId
				var complaint = await _grievanceService.GetGrievanceByIdAsync(complaintId);

				if (complaint != null)
				{
					// Update the complaint status
					complaint.Status = status;

                    await _grievanceService.UpdateAsync(complaint);

					TempData["SuccessMessage"] = "Complaint status updated successfully.";
					return RedirectToAction("Details", new { id = complaintId });
				}
				else
				{
					TempData["ErrorMessage"] = "Complaint not found.";
					return RedirectToAction("Details", new { id = complaintId });
				}
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "An error occurred while updating the status.";
				return RedirectToAction("Details", new { id = complaintId });
			}
		}


		/// <summary>
		/// Adds a new timeline entry for a specific grievance.
		/// </summary>
		/// <param name="grievanceId">The grievance ID</param>
		/// <param name="date">The date of the work entry</param>
		/// <param name="work">Description of the work done</param>
		[HttpPost]
        public async Task<IActionResult> AddTimeLineEntry(int grievanceId, DateTime date, string work)
        {
            var grievance = await _grievanceService.GetGrievanceByIdAsync(grievanceId);

            if (grievance != null)
            {
                await _officerService.AddTimeLineEntryAsync(grievance, date, work);
            }

            return RedirectToAction("Details", new { id = grievanceId }); // Redirect back to the grievance details page
        }

        /// <summary>
        /// Adds a reason for the grievance status, along with a timeline entry.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddReason(int grievanceId, DateTime date, string work)
        {
            try
            {
                var grievance = await _grievanceService.GetGrievanceByIdAsync(grievanceId);

                if (grievance == null)
                {
                    return Json(new { success = false, message = "Grievance not found." }); // Return error if grievance not found
                }

                // Deserialize or initialize timeline
                await _officerService.AddTimeLineEntryAsync(grievance, date, work);

                return Json(new { success = true, message = "Timeline entry added successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while adding the timeline entry.", error = ex.Message });
            }
        }

        /// <summary>
        /// Logs the officer out of the system.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
			await _citizenService.SignOutAsync();
			return RedirectToAction("Login"); // Redirect to login page after logout
        }

        [HttpGet]
        public IActionResult Register()
        {
            
            return View(new OfficerRegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(OfficerRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _officerService.CreateAsync(model);
                
                return View(new OfficerRegisterViewModel());
            }
            return View(model);
        }
    }
}
