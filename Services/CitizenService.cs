using System.Security.Claims;
using GovConnect.Data;
using GovConnect.Models;

namespace GovConnect.Services
{
    public class CitizenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SqlServerDbContext _context; 

        public CitizenService(IHttpContextAccessor httpContextAccessor, SqlServerDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public Citizen GetLoggedInUser()
        {
            // Get the UserId from the authenticated user's claims
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return null; // User is not logged in, return null or handle accordingly
            }

            // Retrieve the user from the database using the UserId
            var user = _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new Citizen
                {
                    Profilepic = u.Profilepic,
                    UserName = u.UserName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    Gender = u.Gender,
                    Email = u.Email,
                    Pincode = u.Pincode,
                    District = u.District,
                    Mandal = u.Mandal,
                    City = u.City,
                    Village = u.Village
                })
                .FirstOrDefault();

            return user;
        }
    }

}
