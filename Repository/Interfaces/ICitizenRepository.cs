using System.Security.Claims;

namespace GovConnect.Repository.Interfaces
{
    public interface ICitizenRepository
    {
        Task<Citizen> GetUserByEmailAsync(string email);
        Task<IdentityResult> CreateUserAsync(Citizen citizen, string password);
        Task<IdentityResult> AddToRoleAsync(Citizen user, string role);
        Task<bool> isInRoleAsync(Citizen user, string role);
        Task<string> GenerateTokenAsync(Citizen user);
        Task<Microsoft.AspNetCore.Identity.SignInResult> PasswordSignInAsync(Citizen user, LoginViewModel model, bool isPersistent, bool lockoutOnFailure);
        Task<IdentityResult> ConfirmEmailAsync(Citizen user, string token);
        Task<IdentityResult> UpdateUserAsync(Citizen citizen);
        Task<Citizen> GetUserAsync(ClaimsPrincipal user);
        Task SignInAsync(Citizen user, bool isPersistent);
        Task<IdentityResult> RemovePasswordAsync(Citizen user);
        Task<IdentityResult> AddPasswordAsync(Citizen user, string password);
        Task SignOutAsync();
    }
}
