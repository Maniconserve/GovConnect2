using System.ComponentModel.DataAnnotations;

namespace GovConnect.Models
{
    public class Grievance
    {
        public int GrievanceID { get; set; }
        public string UserID { get; set; } = ""; // You may adjust this type based on how you're storing UserIDs
        
        public int? OfficerId { get; set; }
        [Required(ErrorMessage = "Department is required")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public byte[]? FilesUploaded { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Automatically set the current timestamp
    }

}
