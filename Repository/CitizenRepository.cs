using System.Security.Claims;
namespace GovConnect.Repository
{
    public class CitizenRepository : ICitizenRepository
    {
        private UserManager<Citizen> _citizenManager;
        private SignInManager<Citizen> _signInManager;

        public CitizenRepository(UserManager<Citizen> citizenManager,SignInManager<Citizen> signInManager)
        {
            _citizenManager = citizenManager;
            _signInManager = signInManager;
        }

        public async Task<Citizen> GetUserByEmailAsync(string email)
        {
            return await _citizenManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> CreateUserAsync(Citizen citizen, string password)
        {
            return await _citizenManager.CreateAsync(citizen, password);
        }

        public async Task<IdentityResult> AddToRoleAsync(Citizen User,String role)
        {
            return await _citizenManager.AddToRoleAsync(User, role);
        }

        public async Task<bool> isInRoleAsync(Citizen User,String role)
        {
            return await _citizenManager.IsInRoleAsync(User,role);
        }

        public async Task<bool> checkPasswordAsync(Citizen citizen,String password)
        {
            return await _citizenManager.CheckPasswordAsync(citizen, password);
        }

        public async Task<String> GenerateTokenAsync(Citizen user)
        {
            return await _citizenManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<Microsoft.AspNetCore.Identity.SignInResult> PasswordSignInAsync(Citizen user, LoginViewModel model, bool isPersistent, bool lockoutOnFailure)
        {
            return await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(Citizen user, string token)
        {
            return await _citizenManager.ConfirmEmailAsync(user, token);
        }

        public async Task<IdentityResult> UpdateUserAsync(Citizen citizen)
        {
            return await _citizenManager.UpdateAsync(citizen);
        }

        public async Task<Citizen> GetUserAsync(ClaimsPrincipal user)
        {
            return await _citizenManager.GetUserAsync(user);
        }

        public async Task SignInAsync(Citizen user, bool isPersistent)
        {
            await _signInManager.SignInAsync(user, isPersistent);
        }

        public async Task<IdentityResult> RemovePasswordAsync(Citizen user)
        {
            return await _citizenManager.RemovePasswordAsync(user);
        }

        public async Task<IdentityResult> AddPasswordAsync(Citizen user, string password)
        {
            return await _citizenManager.AddPasswordAsync(user, password);
        }

        public async Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemes()
        {
            return await _signInManager.GetExternalAuthenticationSchemesAsync();
        }

        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
        {
            return await _signInManager.GetExternalLoginInfoAsync();
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }

}
