using System.ComponentModel.DataAnnotations;

namespace HCMPortal.ViewModel
{
    public class HCMEnquiryViewModel
    {
        [Required(ErrorMessage = "Enter Full Name")]
        public string ProspectName { get; set; }
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email Address is required")]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[a-zA-Z]{2,}(\.[a-zA-Z]{2,})?$", ErrorMessage = "Please enter a valid email address.")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Enter Subject")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Enter Message")]
        public string Message { get; set; }


        [Required(ErrorMessage = "Enter Mobile Number")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter valid 10 digit mobile number")]
        public string PhoneNumber { get; set; }
        public string? EnquiryId { get; set; } = null!;
    }
}
