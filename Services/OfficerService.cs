using System.Text.Json;

namespace GovConnect.Services
{
    public class OfficerService :IOfficerService
    {
        private IGrievanceService _grievanceService;
        private IOfficerRepository _officerRepository;
        public OfficerService(IGrievanceService grievanceService,IOfficerRepository officerRepository) {
            _grievanceService = grievanceService;
            _officerRepository = officerRepository;
        }
        public async Task AddTimeLineEntryAsync(Grievance grievance, DateTime date, string work)
        {
            var timeLine = string.IsNullOrEmpty(grievance.TimeLine)
                    ? new List<TimeLineEntry>()
                    : JsonSerializer.Deserialize<List<TimeLineEntry>>(grievance.TimeLine);

            // Add the new timeline entry
            timeLine.Add(new TimeLineEntry
            {
                Date = date,
                Work = work
            });

            // Update the grievance with the new timeline
            grievance.SetTimeLine(timeLine);
            await _grievanceService.UpdateAsync(grievance);
        }
        public async Task CreateAsync(OfficerRegisterViewModel model)
        {
            var citizen = new Citizen
            {
                UserName = model.FirstName,
                Email = model.Email,
                LastName = model.LastName,
                Gender = model.Gender,
                PhoneNumber = model.Mobile,
                Pincode = model.Pincode,
                Mandal = model.Mandal,
                District = model.District,
                City = model.City,
                Village = model.Village,
                Profilepic = await ConvertFileToByteArray(model.ProfilePic) // Convert uploaded profile picture to byte array.
            };
            PoliceOfficer policeOfficer = new PoliceOfficer
            {
                OfficerId = citizen.Id,
                DepartmentId = model.DepartmentID,
                OfficerDesignation = model.OfficerDesignation,
                SuperiorId = model.SuperiorId
            };
            await _officerRepository.CreateAsync(citizen,model.Password,policeOfficer);
        }

        private async Task<byte[]> ConvertFileToByteArray(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
