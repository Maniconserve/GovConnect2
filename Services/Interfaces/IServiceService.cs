
namespace GovConnect.Services
{
    public interface IServiceService
    {
        Task<List<Service>> GetAllServicesAsync();
        Task<bool> ApplyForServiceAsync(ServiceApplication model, string userId);
        Task<List<ServiceApplication>> GetMyServicesAsync(string userId, Status? statusFilter);
        Task<bool> WithdrawServiceAsync(int applicationId, string userId);
    }
}
