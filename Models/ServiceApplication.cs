namespace GovConnect.Models
{
    public class ServiceApplication
    {
        public int ApplicationID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }
        public int? OfficerID { get; set; }  // Optional: Can be null if not assigned yet
    }

}
