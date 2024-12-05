using GovConnect.Repository.Interfaces;

namespace GovConnect.Repository
{
    public interface IServiceRepository<T> : IGenericRepository<T> where T : class
    {
        Task<List<Service>> GetAllServicesAsync();
        Task<ServiceApplication?> GetServiceApplicationByIdAsync(int applicationId, string userId);
        Task<bool> ApplyForServiceAsync(ServiceApplication application);
        Task<List<ServiceApplication>> GetAppliedServicesAsync(string userId, Status? statusFilter);
        Task<bool> WithdrawServiceAsync(int applicationId, string userId);
    }
}
