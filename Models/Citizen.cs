using Microsoft.AspNetCore.Identity;

namespace GovConnect.Models
{
    public class Citizen : IdentityUser
    {
        public byte[] Profilepic { get; set; }
        public String LastName { get; set; }
        public String Gender { get; set; }
        public int Pincode {  get; set; }
        public String Mandal { get; set; }
        public String District { get; set; }
        public String City { get; set; }
        public String Village { get; set; }
    }
}
