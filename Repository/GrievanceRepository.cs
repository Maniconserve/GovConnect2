using Newtonsoft.Json;

namespace GovConnect.Repository
{
    public class GrievanceRepository : GenericRepository<Grievance>,IGrievanceRepository
    {
        private readonly SqlServerDbContext _context;

        public GrievanceRepository(SqlServerDbContext context) : base(context) {  
            _context = context;
        }

        public async Task<List<Grievance>> GetGrievancesByUserAsync(string userId,Status? statusFilter)
        {
            var grievancesQuery = _context.DGrievances
                .Where(g => g.UserID == userId);

            if (statusFilter.HasValue)
            {
                grievancesQuery = grievancesQuery.Where(g => g.Status == statusFilter.Value);
            }

            return await grievancesQuery.OrderByDescending(g => g.CreatedAt).ToListAsync();
        }

        public async Task<bool> LodgeGrievanceAsync(Grievance grievance)
        {
            _context.DGrievances.Add(grievance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TimeLineEntry>?> GetGrievanceTimelineAsync(int grievanceId)
        {
            var grievance = await _context.DGrievances
                .FirstOrDefaultAsync(g => g.GrievanceID == grievanceId);

            if (grievance == null || string.IsNullOrEmpty(grievance.TimeLine))
                return new List<TimeLineEntry>();

            return JsonConvert.DeserializeObject<List<TimeLineEntry>>(grievance.TimeLine);
        }

        public async Task<List<GrievanceFile>> GetAllGrievanceFilesAsync(int grievanceId)
        {
            // Retrieve all files associated with the given grievance ID
            var grievanceFiles = await _context.DGrievanceFiles
                .Where(f => f.GrievanceID == grievanceId)  // Filter by GrievanceID
                .ToListAsync();  // Get all matching files

            // Return the full list of GrievanceFile objects (not just the file content)
            return grievanceFiles;
        }


        public async Task<PoliceOfficer?> GetSuperiorOfficerByDepartmentAsync(string? officerId)
        {
            return await _context.PoliceOfficers
                .Where(o => o.OfficerId == officerId)
                .FirstOrDefaultAsync();
        }

        public async Task<GrievanceFile> GetFileAsync(int fileId)
        {
            // Retrieve the file from the database based on the fileId
            var file = await _context.DGrievanceFiles
                                     .FirstOrDefaultAsync(f => f.FileID == fileId);

            // Check if the file was found, if not return null
            if (file == null)
            {
                return null;  // or throw an exception if preferred
            }

            return file;
        }


        public async Task UploadFile(GrievanceFile file)
        {
            await _context.DGrievanceFiles.AddAsync(file);
            await _context.SaveChangesAsync();
        }
    }
}
