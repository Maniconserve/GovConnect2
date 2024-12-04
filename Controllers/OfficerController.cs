using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GovConnect.Controllers
{
    public class OfficerController : Controller
    {
        private DashboardService _dashboardService;
        private SqlServerDbContext _SqlServerDbContext;
        private EmailSender _emailSender;
        private static string originalOtp;

        public OfficerController(DashboardService dashboardService, SqlServerDbContext sqlServerDbContext, EmailSender emailSender)
        {
            _dashboardService = dashboardService;
            _SqlServerDbContext = sqlServerDbContext;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Displays the login page if the officer is not authenticated.
        /// Redirects to the Scheme index page if the officer is already logged in.
        /// </summary>
        
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Scheme"); // Redirect to Scheme index if logged in
            }
            return View();
        }

        /// <summary>
        /// Handles the POST request to log in the officer.
        /// Validates officer credentials and sets authentication cookies on success.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Attempt to find the officer by email
                var officer = await _SqlServerDbContext.PoliceOfficers
                    .FirstOrDefaultAsync(o => o.Email != null && o.Email == model.Email);

                if (officer != null)
                {
                    // Validate the password
                    if (officer.Password == model.Password)
                    {
                        // Define claims for the logged-in officer
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, officer.OfficerName),
                            new Claim(ClaimTypes.Email, officer.Email),
                            new Claim("OfficerId", officer.OfficerId.ToString())
                        };

                        // Add a custom role claim for the officer
                        claims.Add(new Claim(ClaimTypes.Role, "NotUser"));

                        var identity = new ClaimsIdentity(claims, "OfficerLogin");
                        var principal = new ClaimsPrincipal(identity);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = false // Set to true if the login should persist across sessions
                        };

                        // Sign the officer in using cookie authentication
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, authProperties);

                        // Redirect to the returnUrl if specified or to the officer dashboard
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Dashboard", "Officer", new { officerId = officer.OfficerId });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "The password is incorrect."); // Invalid password
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Officer doesn't exist."); // Officer not found
                }

                return View(model); // Return view with error messages
            }

            return View(model); // Return view with invalid model state errors
        }

        /// <summary>
        /// Displays the officer dashboard for a specific officer.
        /// </summary>
        /// <param name="officerId">The officer's ID</param>
        [Authorize(Roles = "NotUser", AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [HttpGet]
        public IActionResult Dashboard(int officerId)
        {
            var model = _dashboardService.GetOfficerDashboard(officerId);
            if (model == null)
            {
                return NotFound(); // Return 404 if no dashboard data is found
            }

            return View(model); // Return the dashboard view
        }

        /// <summary>
        /// Displays the details of a specific grievance.
        /// </summary>
        /// <param name="id">The grievance ID</param>
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
        /// Displays the forgot password page.
        /// </summary>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        /// <summary>
        /// Sends an OTP to the officer's email for password reset.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendOtp(string email)
        {
            var user = await _SqlServerDbContext.PoliceOfficers.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                TempData["EmailMessage"] = "No user found with this email.";
                return View("ForgotPassword", new ForgotPasswordViewModel { Email = email });
            }

            // Generate and send OTP
            originalOtp = new Random().Next(100000, 999999).ToString();
            var subject = "Reset Password";
            var body = $"Your OTP code for resetting your password is: {originalOtp}";

            try
            {
                await _emailSender.SendEmailAsync(email, subject, body);
                TempData["EmailMessage"] = "OTP has been sent to your email. Please check your inbox.";
            }
            catch (Exception)
            {
                TempData["EmailMessage"] = "Failed to send OTP. Please try again later.";
            }

            return View("ForgotPassword", new ForgotPasswordViewModel { Email = email });
        }

        /// <summary>
        /// Verifies the OTP entered by the officer and allows resetting the password.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            if (!forgotPasswordViewModel.Otp.Equals(originalOtp))
            {
                TempData["OtpMessage"] = "OTP is invalid";
                return View("ForgotPassword", forgotPasswordViewModel);
            }

            if (ModelState.IsValid)
            {
                var user = await _SqlServerDbContext.PoliceOfficers
                                                   .FirstOrDefaultAsync(u => u.Email == forgotPasswordViewModel.Email);

                if (user != null)
                {
                    user.Password = forgotPasswordViewModel.Password;
                    _SqlServerDbContext.PoliceOfficers.Update(user);
                    var saveResult = await _SqlServerDbContext.SaveChangesAsync();

                    if (saveResult > 0)
                    {
                        return RedirectToAction("Login"); // Redirect to login after successful password reset
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update the password.";
                    }
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Logs the officer out of the system.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Login"); // Redirect to login page after logout
        }
    }
}
