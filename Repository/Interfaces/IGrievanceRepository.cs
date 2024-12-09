namespace GovConnect.Repository
{
    public interface IGrievanceRepository : IGenericRepository<Grievance>
    {
        Task<List<Grievance>> GetGrievancesByUserAsync(string userId, Status? statusFilter);
        Task<bool> LodgeGrievanceAsync(Grievance grievance);
        Task<List<TimeLineEntry>?> GetGrievanceTimelineAsync(int grievanceId);
        Task<bool> UpdateGrievanceFilesAsync(int grievanceId, IFormFile fileUpload);
        Task<byte[]?> GetGrievanceFileAsync(int grievanceId);
        Task<PoliceOfficer?> GetSuperiorOfficerByDepartmentAsync(string? officerId);
    }
}
