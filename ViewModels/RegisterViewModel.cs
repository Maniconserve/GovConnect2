using System.ComponentModel.DataAnnotations;

namespace GovConnect.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First Name is required")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "First Name must contain only letters and spaces.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Last Name must contain only letters and spaces.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [StringLength(15, ErrorMessage = "Mobile number can't be longer than 15 characters.")]
        [Phone(ErrorMessage = "Invalid mobile number.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]
        public string Mobile { get; set; }


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
        [Required(ErrorMessage = "ProfilePic is required")]
        public IFormFile ProfilePic { get; set; }

    }
}
