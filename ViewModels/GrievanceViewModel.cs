namespace GovConnect.ViewModels
{
    public class GrievanceViewModel
    {
        public int GrievanceID { get; set; }
        public int? OfficerId { get; set; }
        public int DepartmentID { get; set; }
        public string Description { get; set; }
        public byte[]? FilesUploaded { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserID { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
    }

}
