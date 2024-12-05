namespace GovConnect.Services
{
    public interface IGrievanceService
    {
        Task<Grievance?> GetGrievanceByIdAsync(int grievanceId);
        Task<List<Grievance>?> GetGrievancesByUserAsync(string userId, Status? statusFilter);
        Task<bool> LodgeGrievanceAsync(Grievance grievance, string userId, IFormFile? fileUpload);
        Task<List<TimeLineEntry>?> GetGrievanceTimelineAsync(int grievanceId);
        Task<bool> UpdateGrievanceFilesAsync(int grievanceId, IFormFile fileUpload);
        Task<bool> EscalateGrievanceAsync(int grievanceId);
        Task<byte[]?> GetGrievanceFileAsync(int grievanceId);
        Task UpdateAsync(Grievance entity);
    }
}
