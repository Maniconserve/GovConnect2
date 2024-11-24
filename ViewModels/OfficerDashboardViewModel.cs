using GovConnect.Models;

namespace GovConnect.ViewModels
{
    public class OfficerDashboardViewModel
    {
        public string OfficerName { get; set; }
        public string OfficerDesignation { get; set; }
        public int DepartmentId { get; set; }
        public int TotalGrievances { get; set; }
        public int PendingGrievances { get; set; }
        public int ResolvedGrievances { get; set; }
        public List<Grievance> AssignedGrievances { get; set; }
        public string OfficerImage { get; set; }
    }
}
