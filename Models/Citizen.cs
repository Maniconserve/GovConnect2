namespace GovConnect.Models
{
    public class Citizen : IdentityUser
    {
        // Adding validation to properties that are not validated in the RegisterViewModel

        [Required(ErrorMessage = "Last Name is required")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Last Name must contain only letters and spaces.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Pincode is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must contain exactly 6 digits.")]
        public int Pincode { get; set; }

        [Required(ErrorMessage = "Mandal is required")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Mandal Name must contain only letters and spaces.")]
        public string Mandal { get; set; }

        [Required(ErrorMessage = "District is required")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "District Name must contain only letters and spaces.")]
        public string District { get; set; }

        [Required(ErrorMessage = "City is required")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "City Name must contain only letters and spaces.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Village is required")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Village Name must contain only letters and spaces.")]
        public string Village { get; set; }

        // ProfilePic is required, but we assume it's handled in RegisterViewModel so no need for validation here.
        public byte[] Profilepic { get; set; }
    }
}
