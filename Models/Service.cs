namespace GovConnect.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }       // Primary Key
        public string ServiceName { get; set; }  // Name of the service
        public int DeptId { get; set; }          // Foreign Key to DDepartment
        public string DocsRequires { get; set; } // Documents required for the service
        public int? ProcessingTime { get; set; } // Processing time in days (nullable)

        // Fee class will be embedded here
        public Fee FeeDetails { get; set; } // This is the fee object

        // Foreign Key to DDepartment
        public Department department { get; set; }
    }

}
