using System.Security.Claims;
using GovConnect.Data;
using GovConnect.Services;
using GovConnect.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GovConnect.Models;
using System.Text.Json;

namespace GovConnect.Controllers
{
    public class OfficerController : Controller
    {
        private DashboardService _dashboardService;
        private SqlServerDbContext _SqlServerDbContext;
        public OfficerController(DashboardService dashboardService, SqlServerDbContext sqlServerDbContext) {
            _dashboardService = dashboardService;
            _SqlServerDbContext = sqlServerDbContext;
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
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
                if (officer != null && officer.Password == model.Password) // In a real application, don't store plain-text passwords
                {
                    // Define the claims for the officer
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, officer.OfficerName),
                        new Claim(ClaimTypes.Email, officer.Email),
                        new Claim("OfficerId", officer.OfficerId.ToString())
                    };

                    // Assign the 'RoleOfficer' role claim if officer has this role
                    // You can add more logic here to check if the officer is in a specific role
                    claims.Add(new Claim(ClaimTypes.Role, "RoleOfficer"));

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
                    // Add error if the officer is not found or the password doesn't match
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
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


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            return RedirectToAction("Login");
        }

    }
}
