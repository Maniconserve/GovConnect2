namespace GovConnect.Repository
{
    public class ServiceRepository : IServiceRepository<Service>
    {
        private readonly SqlServerDbContext _context;

        public ServiceRepository(SqlServerDbContext context)
        {
            _context = context;
        }
        public async Task<ServiceApplication?> GetServiceApplicationByIdAsync(int applicationId, string userId)
        {
            return await _context.ServiceApplications
                .FirstOrDefaultAsync(s => s.ApplicationID == applicationId && s.UserID == userId);
        }
        public async Task<List<Service>> GetAllServicesAsync()
        {
            return await _context.Services.ToListAsync();
        }
        public async Task<List<ServiceApplication>> GetAppliedServicesAsync(string userId, string statusFilter = "All")
        {
            var query = _context.ServiceApplications.Where(s => s.UserID == userId);

            if (statusFilter != "All")
            {
                query = query.Where(s => s.Status == statusFilter);
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

            serviceApplication.Status = "Withdrawn";
            _context.ServiceApplications.Update(serviceApplication);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id);
        }

        public async Task<List<Service>> GetAllAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<bool> AddAsync(Service entity)
        {
            _context.Services.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(Service entity)
        {
            _context.Services.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var service = await GetByIdAsync(id);
            if (service == null) return false;

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
