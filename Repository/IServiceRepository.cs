using GovConnect.Models;

namespace GovConnect.Repository
{
    public interface IServiceRepository
    {
        Task<List<Service>> GetAllServicesAsync();
        Task<List<Service>> GetServicesByDepartmentAsync(string deptName);
        Task<Service> GetServiceByIdAsync(int id);
        Task<ServiceApplication> GetServiceApplicationByIdAsync(int applicationId, string userId);
        Task<bool> ApplyForServiceAsync(ServiceApplication application);
        Task<List<ServiceApplication>> GetAppliedServicesAsync(string userId, string statusFilter);
        Task<bool> WithdrawServiceAsync(int applicationId, string userId);
    }
}
