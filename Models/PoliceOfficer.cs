using System.ComponentModel.DataAnnotations;

namespace GovConnect.Models
{
    public class PoliceOfficer
    {
        [Key]
        public int OfficerId { get; set; }

        [Required]
        [StringLength(100)]
        public string OfficerName { get; set; }

        [Required]
        [StringLength(100)]
        public string OfficerDesignation { get; set; }

        public int? SuperiorId { get; set; } // Nullable as per database

        [Required]
        public int DeptId { get; set; }

        [EmailAddress] // Email validation
        [StringLength(255)] // If you want to enforce the length as per the DB column
        public string Email { get; set; }

        [StringLength(255)] // Adjust password length if needed
        public string Password { get; set; }

        public byte[]? Photo { get; set; } // Nullable as per database (varbinary)

        public Department Department { get; set; }
        public PoliceOfficer Superior { get; set; }
    }
}
