namespace GovConnect.Services
{
    public interface IGrievanceService
    {
        Task<Grievance?> GetGrievanceByIdAsync(int grievanceId);
        Task<List<Grievance>?> GetGrievancesByUserAsync(string userId, Status? statusFilter);
        Task<bool> LodgeGrievanceAsync(Grievance grievance, string userId, List<IFormFile?> fileUpload);
        Task<List<TimeLineEntry>?> GetGrievanceTimelineAsync(int grievanceId);
        Task<bool> UpdateGrievanceFilesAsync(int grievanceId, List<IFormFile?> fileUpload);
        Task<bool> EscalateGrievanceAsync(int grievanceId);
        Task<List<GrievanceFile?>> GetGrievanceFileAsync(int grievanceId);
        Task<GrievanceFile> GetFileAsync(int fileId);
        Task UpdateAsync(Grievance entity);
    }
}
