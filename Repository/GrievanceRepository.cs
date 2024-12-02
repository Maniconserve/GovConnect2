using Newtonsoft.Json;

namespace GovConnect.Repository
{
    public class GrievanceRepository : IGrievanceRepository
    {
        private readonly SqlServerDbContext _context;

        public GrievanceRepository(SqlServerDbContext context)
        {
            _context = context;
        }

        public async Task<Grievance> GetGrievanceByIdAsync(int grievanceId)
        {
            return await _context.DGrievances
                .FirstOrDefaultAsync(g => g.GrievanceID == grievanceId);
        }

        public async Task<List<Grievance>> GetGrievancesByUserAsync(string userId, string statusFilter)
        {
            var grievancesQuery = _context.DGrievances
                .Where(g => g.UserID == userId);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                grievancesQuery = grievancesQuery.Where(g => g.Status == statusFilter);
            }

            return await grievancesQuery.OrderByDescending(g => g.CreatedAt).ToListAsync();
        }

        public async Task<bool> LodgeGrievanceAsync(Grievance grievance)
        {
            _context.DGrievances.Add(grievance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateGrievanceFilesAsync(int grievanceId, IFormFile fileUpload)
        {
            var grievance = await _context.DGrievances
                .FirstOrDefaultAsync(g => g.GrievanceID == grievanceId);

            if (grievance == null)
            {
                return false; // Grievance not found
            }

            if (fileUpload != null && fileUpload.Length > 0)
            {
                // Convert the uploaded file to a byte array
                using (var memoryStream = new MemoryStream())
                {
                    await fileUpload.CopyToAsync(memoryStream);
                    grievance.FilesUploaded = memoryStream.ToArray();
                }

                _context.Update(grievance);
                await _context.SaveChangesAsync();
                return true;
            }

            return false; // No file uploaded
        }

        public async Task<List<TimeLineEntry>> GetGrievanceTimelineAsync(int grievanceId)
        {
            var grievance = await _context.DGrievances
                .FirstOrDefaultAsync(g => g.GrievanceID == grievanceId);

            if (grievance == null || string.IsNullOrEmpty(grievance.TimeLine))
                return new List<TimeLineEntry>();

            return JsonConvert.DeserializeObject<List<TimeLineEntry>>(grievance.TimeLine);
        }

        public async Task<byte[]> GetGrievanceFileAsync(int grievanceId)
        {
            var grievance = await _context.DGrievances
                .FirstOrDefaultAsync(g => g.GrievanceID == grievanceId);

            return grievance?.FilesUploaded;  
        }

        public async Task<PoliceOfficer> GetSuperiorOfficerByDepartmentAsync(int? officerId)
        
        {
            return await _context.PoliceOfficers
                .Where(o => o.OfficerId == officerId)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateGrievanceAsync(Grievance grievance)
        {
            _context.DGrievances.Update(grievance);
            await _context.SaveChangesAsync();
        }
    }
}
