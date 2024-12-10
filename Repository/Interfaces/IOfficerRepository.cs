namespace GovConnect.Repository.Interfaces
{
    public interface IOfficerRepository
    {
        Task CreateAsync(Citizen citizen, String password, PoliceOfficer policeOfficer);
    }
}
