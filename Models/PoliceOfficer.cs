using System.ComponentModel.DataAnnotations;

namespace GovConnect.Models
{
    public class PoliceOfficer
    {
        [Key]
        public int OfficerId { get; set; }
        public string OfficerName { get; set; }
        public string OfficerDesignation { get; set; }
        public int? SuperiorId { get; set; }
        public int DeptId { get; set; }

        public string Email { get; set; }

        public String Password { get; set; }

        public byte[] Photo { get; set; }
    }
}
