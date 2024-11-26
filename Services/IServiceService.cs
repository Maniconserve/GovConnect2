using GovConnect.Models;
namespace GovConnect.Services
{
    public interface IServiceService
    {
        Task<List<Service>> GetAllServicesAsync();
        Task<List<Service>> GetServicesByDepartmentAsync(string deptName);
        Task<Service> GetServiceByIdAsync(int id);
        Task<bool> ApplyForServiceAsync(ServiceApplication model, string userId);
        Task<List<ServiceApplication>> GetMyServicesAsync(string userId, string statusFilter);
        Task<bool> WithdrawServiceAsync(int applicationId, string userId);
    }
}
