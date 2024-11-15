using GovConnect.Models;
using GovConnect.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovConnect.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<Citizen> citizenManager;
        private SignInManager<Citizen> signInManager;

        public AccountController(UserManager<Citizen> _citizenManager,SignInManager<Citizen> _signInManager) {
            citizenManager = _citizenManager;
            signInManager = _signInManager;

        }
        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterViewModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model) {
            if (ModelState.IsValid)
            {
                var existingEmail = await citizenManager.FindByEmailAsync(model.Email);
                if(existingEmail != null)
                {
                    ModelState.AddModelError("Email","This email address is already taken.");
                    return View(model);
                }
                var existingMobile = await citizenManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.Mobile);
                if(existingMobile != null)
                {
                    ModelState.AddModelError("Mobile","This mobile number is already taken.");
                    return View(model);
                }
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

                    Profilepic = await ConvertFileToByteArray(model.ProfilePic) 
                };

                var result = await citizenManager.CreateAsync(citizen, model.Password);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(citizen, isPersistent: false);
                    return RedirectToAction("Index", "Home"); 
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult Login(string? returnUrl) {
            return View();
        }
        [HttpPost]
        public async  Task<IActionResult> Login(LoginViewModel model, string? returnUrl) {
            if (ModelState.IsValid)
            {
                var user = await citizenManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
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
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Citizen doesn't exist");
                    return View(model);
                }
            }
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
