namespace GovConnect.Models
{
    public class PoliceOfficer
    {
        [Key]
        [ForeignKey("Citizen")]
        public string OfficerId { get; set; }


        [Required]
        [StringLength(100)]
        public string OfficerDesignation { get; set; }

        public string? SuperiorId { get; set; } // Nullable as per database

        [Required]
        public int DepartmentId { get; set; }

        public Department Department { get; set; }
        public PoliceOfficer Superior { get; set; }
    }
}
