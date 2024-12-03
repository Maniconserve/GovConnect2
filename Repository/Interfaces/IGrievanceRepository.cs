using GovConnect.Repository.Interfaces;

namespace GovConnect.Repository
{
    public interface IGrievanceRepository<T> : IGenericRepository<T> where T:class 
    {
        Task<Grievance?> GetGrievanceByIdAsync(int grievanceId);
        Task<List<Grievance>> GetGrievancesByUserAsync(string userId, string statusFilter);
        Task<bool> LodgeGrievanceAsync(Grievance grievance);
        Task<List<TimeLineEntry>?> GetGrievanceTimelineAsync(int grievanceId);
        Task<bool> UpdateGrievanceFilesAsync(int grievanceId, IFormFile fileUpload);
        Task<byte[]?> GetGrievanceFileAsync(int grievanceId);
        Task<PoliceOfficer?> GetSuperiorOfficerByDepartmentAsync(int? officerId);
        Task UpdateGrievanceAsync(Grievance grievance);
    }
}
