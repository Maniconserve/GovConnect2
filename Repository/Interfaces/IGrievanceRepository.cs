namespace GovConnect.Repository
{
    public interface IGrievanceRepository : IGenericRepository<Grievance>
    {
        Task<List<Grievance>> GetGrievancesByUserAsync(string userId, Status? statusFilter);
        Task<bool> LodgeGrievanceAsync(Grievance grievance);
        Task<List<TimeLineEntry>?> GetGrievanceTimelineAsync(int grievanceId);
        Task<List<GrievanceFile>> GetAllGrievanceFilesAsync(int grievanceId);
        Task<PoliceOfficer?> GetSuperiorOfficerByDepartmentAsync(string? officerId);
        Task<GrievanceFile> GetFileAsync(int fileId);
        Task UploadFile(GrievanceFile file);
    }
}
