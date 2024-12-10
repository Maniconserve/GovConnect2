using GovConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace GovConnect.Services
{
    public class GrievanceService : IGrievanceService
    {
        private readonly IGrievanceRepository _grievanceRepository;

        public GrievanceService(IGrievanceRepository grievanceRepository)
        {
            _grievanceRepository = grievanceRepository;
        }

        public async Task<Grievance?> GetGrievanceByIdAsync(int grievanceId)
        {
            return await _grievanceRepository.GetByIdAsync(grievanceId);
        }

        public async Task<List<Grievance>?> GetGrievancesByUserAsync(string userId, Status? statusFilter)
        {
            return await _grievanceRepository.GetGrievancesByUserAsync(userId, statusFilter);
        }

        public async Task<bool> LodgeGrievanceAsync(Grievance grievance, string userId, List<IFormFile?>  files)
        {
            // Generate random GrievanceID
            var random = new Random();
            int randomGrievanceId;
            do
            {
                randomGrievanceId = random.Next(100000, 999999); // 6-digit number
            } while (await _grievanceRepository.GetByIdAsync(randomGrievanceId) != null);

            grievance.GrievanceID = randomGrievanceId;
            grievance.UserID = userId;

            // Handle file upload
            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        var fileContent = memoryStream.ToArray();
                        var fileSize = fileContent.Length;

                        var grievanceFile = new GrievanceFile
                        {
                            GrievanceID = randomGrievanceId,
                            FileName = file.FileName,
                            FileContent = fileContent,
                        };

                        // Save the file record to the database
                        await _grievanceRepository.UploadFile(grievanceFile);
                    }
                }
            }
            return await _grievanceRepository.LodgeGrievanceAsync(grievance);
        }

        public async Task<List<TimeLineEntry>?> GetGrievanceTimelineAsync(int grievanceId)
        {
            return await _grievanceRepository.GetGrievanceTimelineAsync(grievanceId);
        }

        public async Task<bool> UpdateGrievanceFilesAsync(int grievanceId,List<IFormFile?> files)
        {
            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        var fileContent = memoryStream.ToArray();
                        var fileSize = fileContent.Length;

                        var grievanceFile = new GrievanceFile
                        {
                            GrievanceID = grievanceId,
                            FileName = file.FileName,
                            FileContent = fileContent,
                        };

                        // Save the file record to the database
                        await _grievanceRepository.UploadFile(grievanceFile);
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<List<GrievanceFile?>> GetGrievanceFileAsync(int grievanceId)
        {
            return await _grievanceRepository.GetAllGrievanceFilesAsync(grievanceId);
        }

        public async Task<GrievanceFile> GetFileAsync(int fileId)
        {
            return await _grievanceRepository.GetFileAsync(fileId);
        }

        public async Task<bool> EscalateGrievanceAsync(int grievanceId)
        {
            var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
            if (grievance == null)
            {
                return false; 
            }

            // Get the superior officer for the grievance's department
            var assignedOfficer = await _grievanceRepository.GetSuperiorOfficerByDepartmentAsync(grievance.OfficerId);

            if (assignedOfficer != null)
            {
                grievance.OfficerId = assignedOfficer.SuperiorId;

                await _grievanceRepository.UpdateAsync(grievance);


                return true; 
            }

            return false; 
        }
        public async Task UpdateAsync(Grievance entity)
        {
            await _grievanceRepository.UpdateAsync(entity);
        }
    }
}
