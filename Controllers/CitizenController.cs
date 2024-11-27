using System.Security.Claims;
using GovConnect.Data;
using GovConnect.Models;
using GovConnect.Services;
using GovConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovConnect.Controllers
{
    public class CitizenController : Controller
    {
        private UserManager<Citizen> citizenManager;
        private SignInManager<Citizen> signInManager;
        private SqlServerDbContext SqlServerDbContext;
        private EmailSender emailSender;
        private static string originalotp;
        public CitizenController(UserManager<Citizen> _citizenManager,SignInManager<Citizen> _signInManager, EmailSender _emailSender, SqlServerDbContext _SqlServerDbContext) {
            citizenManager = _citizenManager;
            signInManager = _signInManager;
            emailSender = _emailSender;
            SqlServerDbContext = _SqlServerDbContext;
            
        }
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Scheme");
            }
            var model = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingEmail = await citizenManager.FindByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "This email address is already taken.");
                    return View(model);
                }
                var existingMobile = await citizenManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.Mobile);
                if (existingMobile != null)
                {
                    ModelState.AddModelError("Mobile", "This mobile number is already taken.");
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
                    Village = model.Village,
                    Profilepic = await ConvertFileToByteArray(model.ProfilePic)
                };

                var result = await citizenManager.CreateAsync(citizen, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Citizen");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Scheme");
            }
            var loginVM = new LoginViewModel()
            {
                Schemes = await signInManager.GetExternalAuthenticationSchemesAsync()
            };
            return View(loginVM);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await citizenManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    if (user.EmailConfirmed == false)
                    {
                        SendEmail(model, user);
                        TempData["Message"] = "Thank you for registering! An email has been sent to your address with a link to log in. Please check your inbox (and spam folder) to proceed with logging in.";
                        model.Schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                        return View(model);
                    }
                    else
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
                                return RedirectToAction("Index", "Scheme");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt");
                            model.Schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                            return View(model);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Citizen doesn't exist");
                    model.Schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                    return View(model);
                }
            }
            return View(model);
        }

        private async void SendEmail(LoginViewModel model, Citizen user)
        {
            var token = await citizenManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "ConfirmEmail", "Citizen",
                new { token, email = user.Email },
                protocol: HttpContext.Request.Scheme);

            await emailSender.SendEmailAsync(
                model.Email,
                "Email Confirmation",
                $"Please confirm your email by clicking here: <a href='{callbackUrl}'>link</a>");
        }

        [HttpGet]
        private async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (email == null || token == null)
            {
                return RedirectToAction("Index", "Scheme");
            }

            var user = await citizenManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Index", "Scheme");
            }

            var result = await citizenManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View("Login", new LoginViewModel { Schemes = await signInManager.GetExternalAuthenticationSchemesAsync() });
            }
            else
            {
                return RedirectToAction("Index", "Scheme");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var user = await citizenManager.GetUserAsync(User);

                if (user == null)
                {
                    return NotFound(); // Return an error if the user is not found
                }
                // Map user data to the Citizen model
                var model = new Citizen
                {
                    UserName = user.UserName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    PhoneNumber = user.PhoneNumber,
                    City = user.City,
                    Email = user.Email,
                    Profilepic = user.Profilepic,
                    Pincode = user.Pincode,
                    Mandal = user.Mandal,
                    District = user.District,
                    Village = user.Village
                };
                return View(model); // Return the view with the model
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., use a logging framework like Serilog or NLog)
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Citizen citizen, IFormFile Profilepic)
        {
            Citizen user = await citizenManager.GetUserAsync(User);

            if (Profilepic != null && Profilepic.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await Profilepic.CopyToAsync(memoryStream);
                    user.Profilepic = memoryStream.ToArray(); // Save the profile picture as byte[]
                }
            }

            // Update other fields if they are not null
            if (citizen.Email != user.Email)
            {
                // Update the email and set EmailConfirmed to false
                user.Email = citizen.Email;
                user.EmailConfirmed = false;
            }
            user.UserName = citizen.UserName ?? user.UserName;
            user.LastName = citizen.LastName ?? user.LastName;
            user.PhoneNumber = citizen.PhoneNumber ?? user.PhoneNumber;
            user.City = citizen.City ?? user.City;
            user.Gender = citizen.Gender ?? user.Gender;
            user.District = citizen.District ?? user.District;
            user.Pincode = citizen.Pincode != 0 ? citizen.Pincode : user.Pincode;
            user.Mandal = citizen.Mandal ?? user.Mandal;
            user.Village = citizen.Village ?? user.Village;

            await citizenManager.UpdateAsync(user);
            return RedirectToAction("Edit"); // Redirect to the profile page or wherever appropriate
        }


        [HttpGet]
        public IActionResult GoogleLogin(String provider,String returnUrl="")
        {
            var redirectUrl = Url.Action("GoogleLoginCallBack", "Citizen", new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            properties.Items["prompt"] = "select_account";
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> GoogleLoginCallBack(string remoteError,String returnUrl = "" )
        {
            var loginVM = new LoginViewModel()
            {
                Schemes = await signInManager.GetExternalAuthenticationSchemesAsync()
            };
            if (remoteError != null)
            {
                ModelState.AddModelError("", $"Error from google login provider : {remoteError}");
                return View("Login", loginVM);
            }
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError("", $"Error from google login provider : {remoteError}");
                return View("Login", loginVM);
            }
            var user = await citizenManager.FindByEmailAsync(info.Principal?.FindFirst(ClaimTypes.Email)?.Value);
            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Scheme");
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

        [HttpGet]
        public IActionResult ForgotPassword() {
            return View(new ForgotPasswordViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> SendOtp(string email)
        {
            // Check if the email exists
            var user = await citizenManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["EmailMessage"] = "No user found with this email.";
                return View("ForgotPassword",new ForgotPasswordViewModel { Email = email});
            }

            // Generate a random OTP
            originalotp = new Random().Next(100000, 999999).ToString();  // Generate a 6-digit OTP

            // Send OTP via email
            var subject = "Reset Password";
            var body = $"Your OTP code resetting password is: {originalotp}";

            try
            {
                await emailSender.SendEmailAsync(email, subject, body);

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
            if (!forgotPasswordViewModel.Otp.Equals(originalotp))
            {
                TempData["OtpMessage"] = "OTP is invalid";
                return View("ForgotPassword", forgotPasswordViewModel);
            }
            if (!ModelState.IsValid)
            {
                return View("ForgotPassword",forgotPasswordViewModel);
            }

            // Update the user's password
            var user = await citizenManager.FindByEmailAsync(forgotPasswordViewModel.Email);
            if (user != null)
            {
                var result = await citizenManager.RemovePasswordAsync(user);  // Remove current password
                if (result.Succeeded)
                {
                    result = await citizenManager.AddPasswordAsync(user,forgotPasswordViewModel.Password);  // Add new password
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Login");  // Redirect to login or wherever needed
                    }
                }
            }
            return RedirectToAction("Index");  // Redirect to form again
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await signInManager.SignOutAsync();

            // Redirect to the homepage or login page
            return RedirectToAction("Index", "Scheme");
        }

        [Route("Citizen/HandleError")]
        public IActionResult HandleError(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }
            return View("Error");
        }
    }
}
