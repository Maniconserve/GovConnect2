namespace GovConnect.Services
{
    public class ProfileService 
    {
        private readonly SqlServerDbContext _context;

        public ProfileService(SqlServerDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetProfilePicAsync(string userName)
        {
            var user = await _context.Users
                .Where(u => u.UserName == userName)
                .FirstOrDefaultAsync();

            // If the user has a profile picture, return it, otherwise return a default image URL
            if (user != null && user.Profilepic != null && user.Profilepic.Length > 0)
            {
                var base64String = Convert.ToBase64String(user.Profilepic);
                return $"data:image/jpeg;base64,{base64String}";
            }

            return "/images/default-profile.jpg";
        }
    }
}
