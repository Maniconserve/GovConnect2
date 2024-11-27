using GovConnect.Models;

namespace GovConnect.Repository
{
    public interface IServiceRepository
    {
        Task<List<Service>> GetAllServicesAsync();
        Task<ServiceApplication> GetServiceApplicationByIdAsync(int applicationId, string userId);
        Task<bool> ApplyForServiceAsync(ServiceApplication application);
        Task<List<ServiceApplication>> GetAppliedServicesAsync(string userId, string statusFilter);
        Task<bool> WithdrawServiceAsync(int applicationId, string userId);
    }
}
