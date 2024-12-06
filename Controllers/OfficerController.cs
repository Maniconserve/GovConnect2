using System.Text.Json;

namespace GovConnect.Controllers
{
    public class OfficerController : Controller
    {
        private SqlServerDbContext _SqlServerDbContext;
        private UserManager<Citizen> _officerManager;
        private SignInManager<Citizen> _signInManager;
        private EmailSender _emailSender;
        private DashboardService _dashboardService;
        private static string originalOtp;

        public OfficerController(SqlServerDbContext sqlServerDbContext, EmailSender emailSender,UserManager<Citizen> officerManager,SignInManager<Citizen> signInManager,DashboardService dashboardService)
        {
            _officerManager = officerManager;
            _SqlServerDbContext = sqlServerDbContext;
            _emailSender = emailSender;
            _signInManager = signInManager;
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
                var user = await _officerManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // Check if the user is in the "Officer" role before attempting to sign in
                    var isOfficer = await _officerManager.IsInRoleAsync(user, "Officer");

                    if (isOfficer)
                    {
                        HttpContext.Session.SetString("officerId", user.Id);
                        // Perform password-based sign-in only if the user is an officer
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);

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
        public IActionResult Dashboard(string officerId)
        {
            var model = _dashboardService.GetOfficerDashboard(officerId);
            model.OfficerName = User.Identity.Name;
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
            var grievance = await _SqlServerDbContext.DGrievances
                .Where(g => g.GrievanceID == id)
                .Select(g => new Grievance
                {
                    GrievanceID = g.GrievanceID,
                    Title = g.Title,
                    OfficerId = g.OfficerId,
                    CreatedAt = g.CreatedAt,
                    Status = g.Status,
                    DepartmentID = g.DepartmentID,
                    Description = g.Description,
                    FilesUploaded = g.FilesUploaded,
                    TimeLine = g.TimeLine
                })
                .FirstOrDefaultAsync();

            if (grievance == null)
            {
                return NotFound(); // Return 404 if grievance not found
            }

            return View(grievance); // Return the grievance details view
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
            var grievance = await _SqlServerDbContext.DGrievances.FindAsync(grievanceId);

            if (grievance != null)
            {
                // Deserialize existing timeline or initialize a new one
                var timeLine = string.IsNullOrEmpty(grievance.TimeLine)
                    ? new List<TimeLineEntry>()
                    : JsonSerializer.Deserialize<List<TimeLineEntry>>(grievance.TimeLine);

                // Add the new timeline entry
                timeLine.Add(new TimeLineEntry
                {
                    Date = date,
                    Work = work
                });

                // Update the grievance with the new timeline
                grievance.SetTimeLine(timeLine);
                _SqlServerDbContext.Update(grievance);

                // Save changes to the database
                await _SqlServerDbContext.SaveChangesAsync();
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
                var grievance = await _SqlServerDbContext.DGrievances.FindAsync(grievanceId);

                if (grievance == null)
                {
                    return Json(new { success = false, message = "Grievance not found." }); // Return error if grievance not found
                }

                // Deserialize or initialize timeline
                var timeLine = string.IsNullOrEmpty(grievance.TimeLine)
                    ? new List<TimeLineEntry>()
                    : JsonSerializer.Deserialize<List<TimeLineEntry>>(grievance.TimeLine);

                // Add the reason/work entry to the timeline
                timeLine.Add(new TimeLineEntry
                {
                    Date = date,
                    Work = work
                });

                // Set the updated timeline
                grievance.SetTimeLine(timeLine);
                _SqlServerDbContext.Update(grievance);

                // Save changes to the database
                await _SqlServerDbContext.SaveChangesAsync();

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
			await _signInManager.SignOutAsync();
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
                // Create a new Citizen object with the registration data.
                var citizen = new Citizen
                {
                    UserName = model.FirstName,
                    Email = model.Email,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    PhoneNumber = model.Mobile,
                    Pincode = model.Pincode,
                    Mandal = model.Mandal,
                    District = model.District,
                    City = model.City,
                    Village = model.Village,
                    Profilepic = await ConvertFileToByteArray(model.ProfilePic) // Convert uploaded profile picture to byte array.
                };

                // Create the new user in the database.
                var result = await _officerManager.CreateAsync(citizen, model.Password);

                // Check if user creation was successful.
                if (result.Succeeded)
                {
                    // Immediately set the email as confirmed after successful registration
                    citizen.EmailConfirmed = true;
                    await _officerManager.UpdateAsync(citizen);  // Save changes to the user record.

                    // Assign the "Officer" role to the user
                    await _officerManager.AddToRoleAsync(citizen, "Officer");

                    // Create and save the PoliceOfficer entity
                    PoliceOfficer policeOfficer = new PoliceOfficer
                    {
                        OfficerId = citizen.Id,
                        DepartmentId = model.DepartmentID,
                        OfficerDesignation = model.OfficerDesignation,
                        SuperiorId = model.SuperiorId
                    };

                    _SqlServerDbContext.PoliceOfficers.Add(policeOfficer);
                    await _SqlServerDbContext.SaveChangesAsync();

                    // Return a new model (perhaps a confirmation or success page)
                    return View(new OfficerRegisterViewModel());
                }

                // Add any errors to the model state for display.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Return the registration view with any validation errors.
            return View(model);
        }


        private async Task<byte[]> ConvertFileToByteArray(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
