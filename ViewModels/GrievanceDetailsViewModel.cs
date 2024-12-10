namespace GovConnect.ViewModels
{
    public class GrievanceDetailsViewModel
    {
        public Grievance Grievance { get; set; }
        public List<GrievanceFile> Files { get; set; }  // List of files for the grievance
    }

}
