namespace GovConnect.Models
{
    public class GrievanceFile
    {
        [Key]
        public int FileID { get; set; }          // Unique file identifier
        public int GrievanceID { get; set; }     // Foreign key to link to DGrievances
        public string FileName { get; set; }     // File name
        public byte[] FileContent { get; set; }  // File content in binary

        public virtual Grievance Grievance { get; set; } 
    }
}
