
using System.Security.Claims;
using System.Text.Json;

namespace GovConnect.Controllers
{
    public class OfficerController : Controller
    {
        private DashboardService _dashboardService;
        private SqlServerDbContext _SqlServerDbContext;
        private EmailSender _emailSender;
        private static string originalOtp;
        public OfficerController(DashboardService dashboardService, SqlServerDbContext sqlServerDbContext,EmailSender emailSender) {
            _dashboardService = dashboardService;
            _SqlServerDbContext = sqlServerDbContext;
            _emailSender = emailSender;
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Scheme");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Find the officer by email
                var officer = await _SqlServerDbContext.PoliceOfficers
                    .FirstOrDefaultAsync(o => o.Email != null && o.Email == model.Email);

                // Check if the officer exists and the password matches
                if (officer != null)
                {
                    // Check if the entered password matches the stored password
                    if (officer.Password == model.Password)
                    {
                        // Define the claims for the officer
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, officer.OfficerName),
                            new Claim(ClaimTypes.Email, officer.Email),
                            new Claim("OfficerId", officer.OfficerId.ToString())
                        };

                        // Assign the 'RoleOfficer' role claim if officer has this role
                        claims.Add(new Claim(ClaimTypes.Role, "NotUser"));

                        var identity = new ClaimsIdentity(claims, "OfficerLogin");
                        var principal = new ClaimsPrincipal(identity);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = false // Set to true if you want the login to persist across browser sessions
                        };

                        // Sign in the officer with the cookie authentication scheme
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, authProperties);

                        // Redirect to the returnUrl or home page
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
                        // If the password is incorrect, add a model error
                        ModelState.AddModelError(string.Empty, "The password is incorrect.");
                    }
                }
                else
                {
                    // If no officer is found with the provided email
                    ModelState.AddModelError(string.Empty, "Officer doesn't exist.");
                }

                // If we reach here, it means either the officer doesn't exist or the password is incorrect
                return View(model);
            }

            // If model state is not valid, return the view with the model.
            return View(model);
        }

        [HttpGet]
        public IActionResult Dashboard(int officerId)
        {
            var model = _dashboardService.GetOfficerDashboard(officerId);
            if (model == null) return NotFound();

            return View(model);
        }
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
                return NotFound();
            }

            return View(grievance);
        }

        // Action to add a new timeline entry
        [HttpPost]
        public async Task<IActionResult> AddTimeLineEntry(int grievanceId, DateTime date, string work)
        {
            var grievance = await _SqlServerDbContext.DGrievances.FindAsync(grievanceId);

            if (grievance != null)
            {
                // Deserialize existing timeline, add the new entry, and serialize it back
                var timeLine = string.IsNullOrEmpty(grievance.TimeLine)
                    ? new List<TimeLineEntry>()
                    : JsonSerializer.Deserialize<List<TimeLineEntry>>(grievance.TimeLine);

                timeLine.Add(new TimeLineEntry
                {
                    Date = date,
                    Work = work
                });

                // Update the timeline in the Grievance object
                grievance.SetTimeLine(timeLine);

                // Save changes to the database
                _SqlServerDbContext.Update(grievance);
                await _SqlServerDbContext.SaveChangesAsync();
            }

            // Redirect back to the grievance details page
            return RedirectToAction("Details", new { id = grievanceId });
        }

        [HttpPost]
        public async Task<IActionResult> AddReason(int grievanceId, DateTime date, string work)
        {
            try
            {
                // Fetch the grievance from the database
                var grievance = await _SqlServerDbContext.DGrievances.FindAsync(grievanceId);

                if (grievance == null)
                {
                    return Json(new { success = false, message = "Grievance not found." });
                }

                // Deserialize the existing timeline or create a new one
                var timeLine = string.IsNullOrEmpty(grievance.TimeLine)
                    ? new List<TimeLineEntry>()
                    : JsonSerializer.Deserialize<List<TimeLineEntry>>(grievance.TimeLine);

                // Add the new timeline entry
                timeLine.Add(new TimeLineEntry
                {
                    Date = date,
                    Work = work
                });

                // Set the updated timeline
                grievance.SetTimeLine(timeLine);

                // Save changes to the database
                _SqlServerDbContext.Update(grievance);
                await _SqlServerDbContext.SaveChangesAsync();

                return Json(new { success = true, message = "Timeline entry added successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception (if necessary) and return a failure response
                return Json(new { success = false, message = "An error occurred while adding the timeline entry.", error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> SendOtp(string email)
        {
            // Check if the email exists
            var user = await _SqlServerDbContext.PoliceOfficers.FirstOrDefaultAsync(u => u.Email == email); ;
            if (user == null)
            {
                TempData["EmailMessage"] = "No user found with this email.";
                return View("ForgotPassword", new ForgotPasswordViewModel { Email = email });
            }

            // Generate a random OTP
            originalOtp = new Random().Next(100000, 999999).ToString();  // Generate a 6-digit OTP

            // Send OTP via email
            var subject = "Reset Password";
            var body = $"Your OTP code resetting password is: {originalOtp}";

            try
            {
                await _emailSender.SendEmailAsync(email, subject, body);

                // Save OTP in TempData for use in subsequent form submissions
                TempData["EmailMessage"] = "OTP has been sent to your email. Please check your inbox (and spam folder).";
            }
            catch (Exception)
            {
                TempData["EmailMessage"] = "Failed to send OTP. Please try again later.";
            }
            return View("ForgotPassword", new ForgotPasswordViewModel { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            // Check if the OTP is correct
            if (!forgotPasswordViewModel.Otp.Equals(originalOtp))
            {
                TempData["OtpMessage"] = "OTP is invalid";
                return View("ForgotPassword", forgotPasswordViewModel);
            }

            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return View("ForgotPassword", forgotPasswordViewModel);
            }

            // Retrieve the user from the database using SqlServerDbContext
            var user = await _SqlServerDbContext.PoliceOfficers
                                               .FirstOrDefaultAsync(u => u.Email == forgotPasswordViewModel.Email);

            if (user != null)
            {
                // Assuming you have a Password field in your PoliceOfficers table.
                user.Password = forgotPasswordViewModel.Password;  // Set the new password

                // Update the user in the database
                _SqlServerDbContext.PoliceOfficers.Update(user);

                // Save the changes
                var saveResult = await _SqlServerDbContext.SaveChangesAsync();

                if (saveResult > 0)
                {
                    // Successfully updated the password, redirect to Login
                    return RedirectToAction("Login");
                }
                else
                {
                    // If saving fails, you might want to show an error message.
                    TempData["ErrorMessage"] = "Failed to update the password.";
                }
            }

            // If user is not found or update fails, redirect to the form again
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            return RedirectToAction("Login");
        }

    }
}
