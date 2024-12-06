using System.Security.Claims;
namespace GovConnect.Services.Interfaces
{
    public interface ICitizenService
    {
        Task<Citizen> GetUserByEmailAsync(string email);
        Task<IdentityResult> RegisterUserAsync(RegisterViewModel model);
        Task<bool> CheckRoleAsync(Citizen user, string role);
        Task<string> GetEmailConfirmationTokenAsync(string email);
        Task<Microsoft.AspNetCore.Identity.SignInResult> SignInUserAsync(Citizen user, LoginViewModel model, bool isPersistent, bool lockoutOnFailure);
        Task SignInAsync(Citizen user, bool isPersistent);
        Task<IdentityResult> ConfirmEmailAsync(Citizen user, string token);
        Task<Citizen> GetUserAsync(ClaimsPrincipal user);
        Task EditProfile(Citizen citizen, IFormFile Profilepic, ClaimsPrincipal user);
        Task<IdentityResult> UpdateUserAsync(Citizen citizen);
        AuthenticationProperties GoogleLogin(string provider, string? redirectUrl);
        Task SendOtpAsync(Citizen user);
        Task<bool> VerifyOtpAsync(string otp);
        Task<IdentityResult> RemoveAndResetPasswordAsync(string email, string newPassword);
        Task SignOutAsync();
    }
}
