using System.Security.Claims;

namespace GovConnect.Services
{
    public class CitizenService : ICitizenService
    {
        private readonly UserManager<Citizen> citizenManager;
        private readonly SignInManager<Citizen> signInManager;
        private readonly EmailSender emailSender;
        private string originalOtp;
        private ICitizenRepository _citizenRepository;

        public CitizenService(ICitizenRepository citizenRepository,UserManager<Citizen> citizenManager, SignInManager<Citizen> signInManager, EmailSender emailSender)
        {
            this.citizenManager = citizenManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            _citizenRepository = citizenRepository;
        }

        public async Task<Citizen> GetUserByEmailAsync(string email)
        {
            return await _citizenRepository.GetUserByEmailAsync(email);
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
        {
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

            await _citizenRepository.CreateUserAsync(citizen, model.Password);
            return await _citizenRepository.AddToRoleAsync(citizen, "User");
        }

        public async Task<bool> CheckRoleAsync(Citizen User, String role)
        {
            return await _citizenRepository.isInRoleAsync(User, role);
        }

        public async Task<String> GetEmailConfirmationTokenAsync(string email)
        {
            var user = await _citizenRepository.GetUserByEmailAsync(email);
            var token = await _citizenRepository.GenerateTokenAsync(user);
            return token;
        }

        public async Task<Microsoft.AspNetCore.Identity.SignInResult> SignInUserAsync(Citizen user,LoginViewModel model,bool isPersistent,bool lockoutOnFailure)
        {
             return await _citizenRepository.PasswordSignInAsync(user, model, false, false);
        }
        public async Task SignInAsync(Citizen user,bool isPersistent)
        {
            await _citizenRepository.SignInAsync(user, isPersistent);
        }
        public async Task<IdentityResult> ConfirmEmailAsync(Citizen user, string token)
        {
            if (user != null)
            {
                return await _citizenRepository.ConfirmEmailAsync(user, token);
            }
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }
        public async Task<Citizen> GetUserAsync(ClaimsPrincipal User)
        {
            return await _citizenRepository.GetUserAsync(User);
        }
        public async Task<bool> EditProfile(Citizen citizen, IFormFile Profilepic,ClaimsPrincipal User)
        {
            Citizen user = await GetUserAsync(User);

            if (Profilepic != null && Profilepic.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await Profilepic.CopyToAsync(memoryStream);
                    user.Profilepic = memoryStream.ToArray(); // Update the profile picture.
                }
            }
            user.UserName = citizen.UserName ?? user.UserName;
            user.LastName = citizen.LastName ?? user.LastName;
            user.PhoneNumber = citizen.PhoneNumber ?? user.PhoneNumber;
            user.City = citizen.City ?? user.City;
            user.Gender = citizen.Gender != null ? citizen.Gender : user.Gender;
            user.District = citizen.District ?? user.District;
            user.Pincode = citizen.Pincode != 0 ? citizen.Pincode : user.Pincode;
            user.Mandal = citizen.Mandal ?? user.Mandal;
            user.Village = citizen.Village ?? user.Village;
            user.Profilepic = citizen.Profilepic ?? user.Profilepic;

            if (citizen.Email != user.Email)
            {
                var emailExists = await _citizenRepository.GetUserByEmailAsync(citizen.Email);
                if (emailExists == null)
                {
                    user.Email = citizen.Email;
                    user.EmailConfirmed = false; // Set email as unconfirmed if changed.
                }
                else
                {
                    return false;
                }
            }

            // Update the user in the database.
            await UpdateUserAsync(user);
            return true;
        }
        public async Task<IdentityResult> UpdateUserAsync(Citizen citizen)
        {
            return await _citizenRepository.UpdateUserAsync(citizen);
        }

        public AuthenticationProperties GoogleLogin(String provider, String? redirectUrl)
        {
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            properties.Items["prompt"] = "select_account";
            return properties;
        }
        public async Task SendOtpAsync(Citizen user, HttpContext httpContext)
        {
            if (user != null)
            {
                originalOtp = new Random().Next(100000, 999999).ToString();
                httpContext.Session.SetString("Otp",originalOtp);
                await emailSender.SendEmailAsync(user.Email, "Reset Password", $"Your OTP code resetting password is: {originalOtp}");
            }
        }

        public Task<string> VerifyOtpAsync(string otp, HttpContext httpContext)
        {
            // Check if the OTP is stored in the session
            string storedOtp = httpContext.Session.GetString("Otp");

            // If the stored OTP is null, it means it has expired
            if (storedOtp == null)
            {
                return Task.FromResult("Expired"); // OTP expired
            }

            // Check if the provided OTP matches the stored OTP
            if (otp == storedOtp)
            {
                return Task.FromResult("Valid"); // OTP is correct
            }
            else
            {
                return Task.FromResult("Invalid"); // OTP is incorrect
            }
        }


        public async Task<IdentityResult> RemoveAndResetPasswordAsync(string email, string newPassword)
        {
            var user = await _citizenRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                await _citizenRepository.RemovePasswordAsync(user);
                return await _citizenRepository.AddPasswordAsync(user, newPassword);
            }
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }

        public async Task SignOutAsync()
        {
            await _citizenRepository.SignOutAsync();
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
        public async Task<IEnumerable<AuthenticationScheme>> GetAuthenticationSchemesAsync() {
            return await _citizenRepository.GetExternalAuthenticationSchemes();
        }
        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
        {
            return await _citizenRepository.GetExternalLoginInfoAsync();
        }
    }
}
