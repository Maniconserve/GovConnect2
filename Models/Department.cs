using System.ComponentModel.DataAnnotations;

namespace GovConnect.Models
{
    public class Department
    {
        [Key]
        public int DeptId { get; set; }          // Primary Key
        public string DeptName { get; set; }     // Department Name
        public string Description { get; set; }  // Department Description

        // Navigation Property for related services
        public ICollection<Service> Services { get; set; }
        public ICollection<PoliceOfficer> PoliceOfficers { get; internal set; }
    }
}
