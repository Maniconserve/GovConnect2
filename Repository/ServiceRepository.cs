using GovConnect.Data;
using GovConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace GovConnect.Repository
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly SqlServerDbContext _context;

        public ServiceRepository(SqlServerDbContext context)
        {
            _context = context;
        }

        public async Task<List<Service>> GetAllServicesAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<ServiceApplication> GetServiceApplicationByIdAsync(int applicationId, string userId)
        {
            return await _context.ServiceApplications
                .FirstOrDefaultAsync(s => s.ApplicationID == applicationId && s.UserID == userId);
        }

        public async Task<bool> ApplyForServiceAsync(ServiceApplication application)
        {
            _context.ServiceApplications.Add(application);
            await _context.SaveChangesAsync();
            return true;
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
    }
}
