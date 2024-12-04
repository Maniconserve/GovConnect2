using System.Security.Claims;
namespace GovConnect.Controllers
{
    public class CitizenController : Controller
    {
        private UserManager<Citizen> citizenManager;
        private SignInManager<Citizen> signInManager;
        private SqlServerDbContext SqlServerDbContext;
        private EmailSender emailSender;
        private static string originalotp;

        public CitizenController(UserManager<Citizen> _citizenManager, SignInManager<Citizen> _signInManager, EmailSender _emailSender, SqlServerDbContext _SqlServerDbContext)
        {
            citizenManager = _citizenManager;
            signInManager = _signInManager;
            emailSender = _emailSender;
            SqlServerDbContext = _SqlServerDbContext;
        }

        /// <summary>
        /// Displays the Registration page if the user is not authenticated.
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            // If the user is already authenticated, redirect them to the Scheme Index page.
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Scheme");
            }
            var model = new RegisterViewModel();
            return View(model);
        }

        /// <summary>
        /// Handles the POST request for user registration.
        /// </summary>
        /// <param name="model">The RegisterViewModel containing the registration form data.</param>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists in the system.
                var existingEmail = await citizenManager.FindByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "This email address is already taken.");
                    return View(model);
                }

                // Check if the mobile number is already associated with another user.
                var existingMobile = await citizenManager.Users.FirstOrDefaultAsync(citizen => citizen.PhoneNumber == model.Mobile);
                if (existingMobile != null)
                {
                    ModelState.AddModelError("Mobile", "This mobile number is already taken.");
                    return View(model);
                }

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
                var result = await citizenManager.CreateAsync(citizen, model.Password);

                // Check if user creation was successful.
                if (result.Succeeded)
                {
                    return RedirectToAction("Login", "Citizen"); // Redirect to login after successful registration.
                }

                // Add any errors to the model state for display.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model); // Return the registration view with any validation errors.
        }

        /// <summary>
        /// Displays the Login page for the user.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl)
        {
            // If the user is already authenticated, redirect them to the Scheme Index page.
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Scheme");
            }
            var loginVM = new LoginViewModel()
            {
                Schemes = await signInManager.GetExternalAuthenticationSchemesAsync() // Get available external authentication schemes (e.g., Google, Facebook).
            };
            return View(loginVM);
        }

        /// <summary>
        /// Handles the POST request for user login.
        /// </summary>
        /// <param name="model">The LoginViewModel containing the login form data.</param>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Find the user by their email.
                var user = await citizenManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // Check if the user's email is confirmed.
                    if (user.EmailConfirmed == false)
                    {
                        // Send confirmation email if not confirmed.
                        SendEmail(model, user);
                        TempData["Message"] = "Thank you for registering! An email has been sent to your address with a link to log in. Please check your inbox (and spam folder) to proceed with logging in.";
                        model.Schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                        return View(model);
                    }
                    else
                    {
                        // Perform password-based sign-in.
                        var result = await signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: false);
                        if (result.Succeeded)
                        {
                            // If login is successful, redirect to the requested return URL or the Scheme Index page.
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
                    // If user doesn't exist, add an error to the model state.
                    ModelState.AddModelError(string.Empty, "Citizen doesn't exist");
                    model.Schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                    return View(model);
                }
            }
            return View(model);
        }

        /// <summary>
        /// Sends an email with the confirmation link for email verification.
        /// </summary>
        private async void SendEmail(LoginViewModel model, Citizen user)
        {
            // Generate a confirmation token for the user.
            var token = await citizenManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "ConfirmEmail", "Citizen",
                new { token, email = user.Email },
                protocol: HttpContext.Request.Scheme);

            // Send the confirmation email with the callback URL.
            await emailSender.SendEmailAsync(
                model.Email,
                "Email Confirmation",
                $"Please confirm your email by clicking here: <a href='{callbackUrl}'>link</a>");
        }

        /// <summary>
        /// Handles the email confirmation callback after the user clicks the confirmation link.
        /// </summary>
        [HttpGet]
        private async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (email == null || token == null)
            {
                return RedirectToAction("Index", "Scheme");
            }

            // Find the user by email.
            var user = await citizenManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Index", "Scheme");
            }

            // Confirm the user's email using the token.
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

        /// <summary>
        /// Displays the user's profile for editing.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var user = await citizenManager.GetUserAsync(User);

                if (user == null)
                {
                    return NotFound();
                }

                // Create a model to pre-populate the edit form with existing user data.
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

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Handles the profile update action (POST request).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(Citizen citizen, IFormFile Profilepic)
        {
            Citizen user = await citizenManager.GetUserAsync(User);

            if (Profilepic != null && Profilepic.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await Profilepic.CopyToAsync(memoryStream);
                    user.Profilepic = memoryStream.ToArray(); // Update the profile picture.
                }
            }

            // Update other user properties (if they have been modified).
            if (citizen.Email != user.Email)
            {
                user.Email = citizen.Email;
                user.EmailConfirmed = false; // Set email as unconfirmed if changed.
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

            // Update the user in the database.
            await citizenManager.UpdateAsync(user);
            return RedirectToAction("Edit");
        }

        /// <summary>
        /// Initiates Google login.
        /// </summary>
        [HttpGet]
        public IActionResult GoogleLogin(String provider, String returnUrl = "")
        {
            var redirectUrl = Url.Action("GoogleLoginCallBack", "Citizen", new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            properties.Items["prompt"] = "select_account";
            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// Handles the callback after a successful Google login.
        /// </summary>
        public async Task<IActionResult> GoogleLoginCallBack(string remoteError, String returnUrl = "")
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
            await signInManager.SignInAsync(user, isPersistent: false); // Sign in the user.
            return RedirectToAction("Index", "Scheme");
        }

        /// <summary>
        /// Displays the Forgot Password page.
        /// </summary>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        /// <summary>
        /// Sends an OTP to the user's email for password reset.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendOtp(string email)
        {
            var user = await citizenManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["EmailMessage"] = "No user found with this email.";
                return View("ForgotPassword", new ForgotPasswordViewModel { Email = email });
            }

            // Generate a random OTP for password reset.
            originalotp = new Random().Next(100000, 999999).ToString();
            var subject = "Reset Password";
            var body = $"Your OTP code resetting password is: {originalotp}";

            try
            {
                // Send OTP email.
                await emailSender.SendEmailAsync(email, subject, body);
                TempData["EmailMessage"] = "OTP has been sent to your email. Please check your inbox (and spam folder).";
            }
            catch (Exception)
            {
                TempData["EmailMessage"] = "Failed to send OTP. Please try again later.";
            }
            return View("ForgotPassword", new ForgotPasswordViewModel { Email = email });
        }

        /// <summary>
        /// Verifies the OTP entered by the user for password reset.
        /// </summary>
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
                return View("ForgotPassword", forgotPasswordViewModel);
            }

            // Find the user and reset their password.
            var user = await citizenManager.FindByEmailAsync(forgotPasswordViewModel.Email);
            if (user != null)
            {
                var result = await citizenManager.RemovePasswordAsync(user);
                if (result.Succeeded)
                {
                    result = await citizenManager.AddPasswordAsync(user, forgotPasswordViewModel.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Login");
                    }
                }
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Logs out the user and redirects to the Scheme Index page.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Scheme");
        }

        /// <summary>
        /// Converts an uploaded file to a byte array.
        /// </summary>
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

        /// <summary>
        /// Routes the user based on their role to either their Officer Dashboard or the Scheme Index page.
        /// </summary>
        public IActionResult Route()
        {
            var userRoles = User.FindAll(ClaimTypes.Role).Select(role => role.Value).ToList();

            // If the user is an officer (not a regular user), redirect to the officer dashboard.
            if (userRoles.Contains("NotUser"))
            {
                var officeId = User.FindFirst("OfficerId")?.Value;

                if (officeId != null)
                {
                    return RedirectToAction("Dashboard", "Officer", new { officerId = officeId });
                }
            }
            return RedirectToAction("Index", "Scheme");
        }

        /// <summary>
        /// Handles errors and returns the appropriate error page.
        /// </summary>
        [Route("Citizen/HandleError")]
        public IActionResult HandleError(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound"); // If the error is 404, show the Not Found page.
            }
            else if (statusCode == 400)
            {
                return View("BadRequest"); // If the error is 400, show the BadRequest page.
            }
            else if(statusCode == 500)
            {
                return View("ServerError"); // If the error is 500, show the ServerError page.
            }
            return View("Error"); // Otherwise, show a generic error page.
        }
    }
}
