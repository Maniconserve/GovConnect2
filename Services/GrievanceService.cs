using GovConnect.Models;
using GovConnect.ViewModels;
using GovConnect.Repository;

namespace GovConnect.Services
{
    public class GrievanceService : IGrievanceService
    {
        private readonly IGrievanceRepository _grievanceRepository;

        public GrievanceService(IGrievanceRepository grievanceRepository)
        {
            _grievanceRepository = grievanceRepository;
        }

        public async Task<Grievance> GetGrievanceByIdAsync(int grievanceId)
        {
            return await _grievanceRepository.GetGrievanceByIdAsync(grievanceId);
        }

        public async Task<List<Grievance>> GetGrievancesByUserAsync(string userId, string statusFilter="All")
        {
            return await _grievanceRepository.GetGrievancesByUserAsync(userId, statusFilter);
        }

        public async Task<bool> LodgeGrievanceAsync(Grievance grievance, string userId, IFormFile? fileUpload)
        {
            // Generate random GrievanceID
            var random = new Random();
            int randomGrievanceId;
            do
            {
                randomGrievanceId = random.Next(100000, 999999); // 6-digit number
            } while (await _grievanceRepository.GetGrievanceByIdAsync(randomGrievanceId) != null);

            grievance.GrievanceID = randomGrievanceId;
            grievance.UserID = userId;

            // Handle file upload
            if (fileUpload != null && fileUpload.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await fileUpload.CopyToAsync(memoryStream);
                    grievance.FilesUploaded = memoryStream.ToArray();
                }
            }

            return await _grievanceRepository.LodgeGrievanceAsync(grievance);
        }

        public async Task<List<TimeLineEntry>> GetGrievanceTimelineAsync(int grievanceId)
        {
            return await _grievanceRepository.GetGrievanceTimelineAsync(grievanceId);
        }

        public async Task<bool> UpdateGrievanceFilesAsync(int grievanceId, IFormFile fileUpload)
        {
            return await _grievanceRepository.UpdateGrievanceFilesAsync(grievanceId, fileUpload);
        }

        public async Task<byte[]> GetGrievanceFileAsync(int grievanceId)
        {
            return await _grievanceRepository.GetGrievanceFileAsync(grievanceId);
        }

        public async Task<bool> EscalateGrievanceAsync(int grievanceId)
        {
            var grievance = await _grievanceRepository.GetGrievanceByIdAsync(grievanceId);
            if (grievance == null)
            {
                return false; 
            }

            // Get the superior officer for the grievance's department
            var assignedOfficer = await _grievanceRepository.GetSuperiorOfficerByDepartmentAsync(grievance.OfficerId);

            if (assignedOfficer != null)
            {
                grievance.OfficerId = assignedOfficer.SuperiorId;

                await _grievanceRepository.UpdateGrievanceAsync(grievance);


                return true; 
            }

            return false; 
        }
    }
}
