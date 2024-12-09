namespace GovConnect.Repository
{
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        private readonly SqlServerDbContext _context;

        public ServiceRepository(SqlServerDbContext context) : base(context) 
        {
            _context = context;
        }
        public async Task<ServiceApplication?> GetServiceApplicationByIdAsync(int applicationId, string userId)
        {
            return await _context.ServiceApplications
                .FirstOrDefaultAsync(s => s.ApplicationID == applicationId && s.UserID == userId);
        }
        public async Task<List<ServiceApplication>> GetAppliedServicesAsync(string userId, Status? statusFilter)
        {
            var query = _context.ServiceApplications.Where(service => service.UserID == userId);

            if (statusFilter.HasValue)
            {
                query = query.Where(service => service.Status == statusFilter.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> ApplyForServiceAsync(ServiceApplication application)
        {
            _context.ServiceApplications.Add(application);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> WithdrawServiceAsync(int applicationId, string userId)
        {
            var serviceApplication = await GetServiceApplicationByIdAsync(applicationId, userId);

            if (serviceApplication == null)
            {
                return false;
            }

            serviceApplication.Status = Status.Withdrawn;
            _context.ServiceApplications.Update(serviceApplication);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
