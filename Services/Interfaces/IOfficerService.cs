namespace GovConnect.Services.Interfaces
{
    public interface IOfficerService
    {
        Task AddTimeLineEntryAsync(Grievance grievance, DateTime date, string work);
        Task CreateAsync(OfficerRegisterViewModel model);
    }
}
