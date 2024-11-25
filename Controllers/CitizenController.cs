using System.Security.Claims;
using GovConnect.Data;
using GovConnect.Models;
using GovConnect.Services;
using GovConnect.ViewModels;
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
        private CitizenService citizenService;
        private EmailSender emailSender;
        private static string originalotp;
        public CitizenController(UserManager<Citizen> _citizenManager,SignInManager<Citizen> _signInManager, EmailSender _emailSender, SqlServerDbContext _SqlServerDbContext, CitizenService _citizenService) {
            citizenManager = _citizenManager;
            signInManager = _signInManager;
            emailSender = _emailSender;
            SqlServerDbContext = _SqlServerDbContext;
            citizenService = _citizenService;
            
        }
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirect to a different page (e.g., Home or Dashboard)
                return RedirectToAction("Index", "Home");
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
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
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

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
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
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            return View(model);
                        }
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

        public async void SendEmail(LoginViewModel model, Citizen user)
        {
            var token = await citizenManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "ConfirmEmail", "Account",
                new { token, email = user.Email },
                protocol: HttpContext.Request.Scheme);

            await emailSender.SendEmailAsync(
                model.Email,
                "Email Confirmation",
                $"Please confirm your email by clicking here: <a href='{callbackUrl}'>link</a>");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (email == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await citizenManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await citizenManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View("Login", new LoginViewModel { Schemes = await signInManager.GetExternalAuthenticationSchemesAsync() });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Citizen citizen)
        {
            Citizen user = await citizenManager.GetUserAsync(User);
            // Step 2: Compare the current email with the new email from the form
            if (citizen.Email != user.Email)
            {
                // Update the email and set EmailConfirmed to false
                user.Email = citizen.Email;
                user.EmailConfirmed = false;
                
            }
            user.UserName = citizen.UserName == null ? user.UserName : citizen.UserName;
            user.LastName = citizen.LastName == null ? user.LastName : citizen.LastName;
            user.PhoneNumber = citizen.PhoneNumber == null ? user.PhoneNumber : citizen.PhoneNumber;
            user.City = citizen.City == null ? user.City : citizen.City;
            user.Gender = citizen.Gender == null ? user.Gender : citizen.Gender;
            user.District = citizen.District == null ? user.District : citizen.District;
            user.Pincode = citizen.Pincode == null ? user.Pincode : citizen.Pincode;
            user.Mandal = citizen.Mandal == null ? user.Mandal : citizen.Mandal;
            user.Village = citizen.Village == null ? user.Village : citizen.Village;
            user.Profilepic = citizen.Profilepic == null ? user.Profilepic : citizen.Profilepic;
            await citizenManager.UpdateAsync(user);
            return RedirectToAction("Edit"); // Redirect to the profile page or wherever appropriate
        }

        [HttpGet]
        public IActionResult GoogleLogin(String provider,String returnUrl="")
        {
            var redirectUrl = Url.Action("GoogleLoginCallBack", "Account", new { ReturnUrl = returnUrl });
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
            return RedirectToAction("Index", "Home");
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
                TempData["OtpMessage"] = "No user found with this email.";
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

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await signInManager.SignOutAsync();

            // Redirect to the homepage or login page
            return RedirectToAction("Index", "Home");
        }

        [Route("Account/HandleError")]
        public IActionResult HandleError(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }

            // Return a generic error page
            return View("Error");
        }
    }
}
