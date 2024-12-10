namespace GovConnect.Repository
{
    public class OfficerRepository : IOfficerRepository
    {
        private UserManager<Citizen> _officerManager;
        private SqlServerDbContext _SqlServerDbContext;
        public OfficerRepository(UserManager<Citizen> officerManager, SqlServerDbContext sqlServerDbContext) { 
            _officerManager = officerManager;
            _SqlServerDbContext = sqlServerDbContext;
        }
        public async Task CreateAsync(Citizen citizen, String password, PoliceOfficer policeOfficer)
        {
            var result = await _officerManager.CreateAsync(citizen, password);
            if (result.Succeeded)
            {
                // Immediately set the email as confirmed after successful registration
                citizen.EmailConfirmed = true;
                await _officerManager.UpdateAsync(citizen);  // Save changes to the user record.

                // Assign the "Officer" role to the user
                await _officerManager.AddToRoleAsync(citizen, "Officer");
                _SqlServerDbContext.PoliceOfficers.Add(policeOfficer);
                await _SqlServerDbContext.SaveChangesAsync();
            }
        }
    }
}
