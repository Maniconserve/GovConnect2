using System.Security.Claims;
using GovConnect.Data;
using GovConnect.Services;
using GovConnect.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                var officer = await _SqlServerDbContext.PoliceOfficers.FirstOrDefaultAsync(o => o.Email == model.Email);

                // Check if the officer exists and password matches
                if (officer != null)
                {
                    // Check if officer's password is not null and matches
                    if (officer.Password != null && officer.Password == model.Password)
                    {
                        var claims = new List<Claim> 
                {
                    new Claim(ClaimTypes.Name, officer.OfficerName),
                    new Claim(ClaimTypes.Email, officer.Email),
                    new Claim("OfficerId", officer.OfficerId.ToString())
                };

                        var identity = new ClaimsIdentity(claims, "OfficerLogin");
                        var principal = new ClaimsPrincipal(identity);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = false // Set to true if you want the login to persist across browser sessions
                        };

                        // Sign the officer in
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                        // Redirect to the returnUrl or home page
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        // Add error if the officer's password is null or doesn't match
                        ModelState.AddModelError(string.Empty, "Invalid login attempt: Password mismatch.");
                        return View(model);
                    }
                }
                else
                {
                    // Add error if the officer is not found
                    ModelState.AddModelError(string.Empty, "Invalid login attempt: Officer not found.");
                    return View(model);
                }
            }

            return View(model);
        }


        [Authorize(Policy = "NotUser")]
        [HttpGet]
        public IActionResult Dashboard(int officerId)
        {
            var model = _dashboardService.GetOfficerDashboard(officerId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

    }
}
