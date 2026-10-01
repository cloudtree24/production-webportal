using System.ComponentModel.DataAnnotations;

namespace HCMPortal.ViewModel
{
    public class DemoBookingViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        public string? Name { get; set; }
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email Address is required")]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[a-zA-Z]{2,}(\.[a-zA-Z]{2,})?$", ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Phone Number is required")]
        //[RegularExpression(@"^(?:[6-9]\d{9}|\+[1-9]\d{1,14})$", ErrorMessage = "Please enter phone number in E.164 format (e.g., +919876543210).")]
        [RegularExpression(@"^(?:[6-9]\d{9}|[1-9]\d{1,14})$", ErrorMessage = "Please enter phone number in E.164 format without '+' (e.g., 919876543210).")]
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int SourceId { get; set; }

        public int CountryId { get; set; }
        //[Display(Name = "State")]
        public int StateId { get; set; }
        public string? Remarks { get; set; }
        //[Display(Name = "Industry Type")]
        public int IndustryTypeId { get; set; }
        //[Display(Name = "Slot Date")]
        public DateTime SlotDate { get; set; } = DateTime.Now;
        public TimeSpan SlotTime { get; set; }
        public int slottimeId { get; set; }

    }
}
