namespace GovConnect.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceService(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<List<Service>> GetAllServicesAsync()
        {
            return await _serviceRepository.GetAllServicesAsync();
        }

        public async Task<bool> ApplyForServiceAsync(ServiceApplication model, string userId)
        {
            model.UserID = userId;
            model.Status = "Pending";
            return await _serviceRepository.ApplyForServiceAsync(model);
        }

        public async Task<List<ServiceApplication>> GetMyServicesAsync(string userId, string statusFilter = "All")
        {
            return await _serviceRepository.GetAppliedServicesAsync(userId, statusFilter);
        }

        public async Task<bool> WithdrawServiceAsync(int applicationId, string userId)
        {
            return await _serviceRepository.WithdrawServiceAsync(applicationId, userId);
        }
    }
}
