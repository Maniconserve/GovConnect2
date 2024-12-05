namespace GovConnect.Models
{
    public class ServiceApplication
    {
        [Key]
        public int ApplicationID { get; set; }
        public String UserID { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public DateTime ApplicationDate { get; set; }
        public Status Status { get; set; } = Status.Pending;
        public int? OfficerID { get; set; }  // Optional: Can be null if not assigned yet
    }

}
