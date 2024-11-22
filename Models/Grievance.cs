using System.ComponentModel.DataAnnotations;

namespace GovConnect.Models
{
    public class Grievance
    {
        public int GrievanceID { get; set; }

        [Required]
        public string UserID { get; set; }  // You may adjust this type based on how you're storing UserIDs

        public int OfficerId { get; set; }

        [Required]
        public int DepartmentID { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string FilesUploaded { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Automatically set the current timestamp
    }

}
