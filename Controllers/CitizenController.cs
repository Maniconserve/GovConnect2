using System.Security.Claims;

namespace GovConnect.Controllers
{
    public class CitizenController : Controller
    {
        private SignInManager<Citizen> signInManager;
        private EmailSender emailSender;
        private ICitizenService _citizenService;
        static DateTime expirationTime; // OTP expires in 5 minutes

        public CitizenController(ICitizenService citizenService,UserManager<Citizen> _citizenManager, SignInManager<Citizen> _signInManager, EmailSender _emailSender, SqlServerDbContext _SqlServerDbContext)
        {
            signInManager = _signInManager;
            emailSender = _emailSender;
            _citizenService = citizenService;
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
                var existingEmail = await _citizenService.GetUserByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "This email address is already taken.");
                    return View(model);
                }

                // Check if the mobile number is already associated with another user.
                //var existingMobile = await citizenManager.Users.FirstOrDefaultAsync(citizen => citizen.PhoneNumber == model.Mobile);
                //if (existingMobile != null)
                //{
                //    ModelState.AddModelError("Mobile", "This mobile number is already taken.");
                //    return View(model);
                //}

                // Create a new Citizen object with the registration data.
                

                // Create the new user in the database.
                var result = await _citizenService.RegisterUserAsync(model);

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
                var user = await _citizenService.GetUserByEmailAsync(model.Email);

                if (user != null)
                {
                    // Check if the user has the required role (e.g., Citizen, Officer, etc.)
                    var isUserRole = await _citizenService.CheckRoleAsync(user, "User"); // Replace "Citizen" with the required role

                    if (!isUserRole)
                    {
                        // If the user does not have the required role, add an error and return the view.
                        ModelState.AddModelError(string.Empty, "You are not an User");
                        model.Schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                        return View(model);
                    }

                    // Now, check if the user's email is confirmed.
                    if (user.EmailConfirmed == false)
                    {
                        String token = await _citizenService.GetEmailConfirmationTokenAsync(user.Email);
                        SendEmail(model, user, token);
                        TempData["Message"] = "Thank you for registering! An email has been sent to your address with a link to log in. Please check your inbox (and spam folder) to proceed with logging in.";
                        model.Schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                        return View(model);
                    }
                    else
                    {
                        // Perform password-based sign-in.
                        var result = await _citizenService.SignInUserAsync(user, model, false, false);
                        if (result.Succeeded)
                        {
                            // If login is successful, redirect to the requested return URL or the default page (Scheme Index page).
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
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            model.Schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                            return View(model);
                        }
                    }
                }
                else
                {
                    // If user doesn't exist, add an error to the model state.
                    ModelState.AddModelError(string.Empty, "Citizen doesn't exist.");
                    model.Schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
                    return View(model);
                }
            }

            // If the model is invalid, return the same view with validation errors.
            return View(model);
        }


        /// <summary>
        /// Sends an email with the confirmation link for email verification.
        /// </summary>
        private async void SendEmail(LoginViewModel model, Citizen user,String token)
        {
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
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            if (email == null || token == null)
            {
                return RedirectToAction("Index", "Scheme");
            }

            // Find the user by email.
            var user = await _citizenService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Index", "Scheme");
            }

            // Confirm the user's email using the token.
            var result = await _citizenService.ConfirmEmailAsync(user, token);
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
                Citizen user = await _citizenService.GetUserAsync(User);

                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
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
            bool result = await _citizenService.EditProfile(citizen, Profilepic, User);
            if (!result)
            {
                ModelState.AddModelError("", "Sorry, Email already exists");
                return View(citizen);
            }
            return RedirectToAction("Edit");
        }

        /// <summary>
        /// Initiates Google login.
        /// </summary>
        [HttpGet]
        public IActionResult GoogleLogin(String provider, String returnUrl = "")
        {
            var redirectUrl = Url.Action("GoogleLoginCallBack", "Citizen", new { ReturnUrl = returnUrl });
            AuthenticationProperties properties = _citizenService.GoogleLogin(provider, redirectUrl);
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

            var user = await _citizenService.GetUserByEmailAsync(info.Principal?.FindFirst(ClaimTypes.Email)?.Value);
            await _citizenService.SignInAsync(user, isPersistent: false); // Sign in the user.
            return RedirectToAction("Index", "Scheme");
        }

        /// <summary>
        /// Displays the Forgot Password page.
        /// </summary>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            var email = HttpContext.Session.GetString("Email");
            if (email != null)
            {
                TempData["EmailMessage"] = "OTP has been sent to your email. Please check your inbox (and spam folder).";
                var countdownTime = (int)(expirationTime - DateTime.UtcNow).TotalSeconds;
                TempData["OtpExpirationTime"] = expirationTime.ToString("o"); // ISO 8601 format
                TempData["OtpCountdownTime"] = countdownTime.ToString();
            }
            return View(new ForgotPasswordViewModel { Email = email });

        }

        /// <summary>
        /// Sends an OTP to the user's email for password reset
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendOtp(string email,String countdown)
        {
            var user = await _citizenService.GetUserByEmailAsync(email);
            if (user == null)
            {
                TempData["EmailMessage"] = "No user found with this email.";
                return View("ForgotPassword", new ForgotPasswordViewModel { Email = email });
            }

            try
            {
                // Send OTP email.
                await _citizenService.SendOtpAsync(user, HttpContext);
                HttpContext.Session.SetString("Email", email);
                if(int.Parse(countdown) <= 0) expirationTime = DateTime.UtcNow.AddSeconds(30);
                var countdownTime = (int)(expirationTime - DateTime.UtcNow).TotalSeconds;
                TempData["OtpExpirationTime"] = expirationTime.ToString("o"); // ISO 8601 format
                TempData["OtpCountdownTime"] = countdownTime.ToString();
                TempData["EmailMessage"] = "OTP has been sent to your email. Please check your inbox (and spam folder).";
                // Store timer data in TempData (e.g., set countdown to 60 seconds).
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
            if (!await _citizenService.VerifyOtpAsync(forgotPasswordViewModel.Otp, HttpContext))
            {
                TempData["OtpMessage"] = "OTP is invalid";
                return View("ForgotPassword",new ForgotPasswordViewModel());
            }

            if (!ModelState.IsValid)
            {
                return View("ForgotPassword",new ForgotPasswordViewModel());
            }

            // Find the user and reset their password.
            var result = await _citizenService.RemoveAndResetPasswordAsync(forgotPasswordViewModel.Email, forgotPasswordViewModel.Password);
       
            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }
            return RedirectToAction("Index", "Scheme");
        }

        /// <summary>
        /// Logs out the user and redirects to the Scheme Index page.
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _citizenService.SignOutAsync();
            return RedirectToAction("Index", "Scheme");
        }

        

        /// <summary>
        /// Routes the user based on their role to either their Officer Dashboard or the Scheme Index page.
        /// </summary>
        public async Task<IActionResult> Route()
        {
            var officer = User.IsInRole("Officer");

            // If the user is an officer (not a regular user), redirect to the officer dashboard.
            if (officer)
            {
                var officerId = HttpContext.Session.GetString("officerId");

                // If the officerId is found, redirect to the officer dashboard.
                if (!string.IsNullOrEmpty(officerId))
                {
                    return RedirectToAction("Dashboard", "Officer", new { officerId = officerId });
                }
            }

            // If not an officer or officerId is not found, redirect to the Scheme Index page
            return RedirectToAction("Index", "Scheme");
        }


        /// <summary>
        /// Handles errors and returns the appropriate error page.
        /// </summary>
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
            else if (statusCode == 500)
            {
                return View("ServerError"); // If the error is 500, show the ServerError page.
            }
            return View("Error"); // Otherwise, show a generic error page.
        }
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }
    }
}