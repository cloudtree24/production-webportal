using System.ComponentModel.DataAnnotations;

namespace HCMPortal.ViewModel
{
    public class ResetPasswordViewModel
    {
        public string? Email { get; set; }
        public string? Token { get; set; }
        [Required(ErrorMessage = "New Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string? NewPassword { get; set; }
        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }
}
