using System.ComponentModel.DataAnnotations;

namespace HCMPortal.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email Address is required")]
        [Display(Name = "Email")]
        public string PortalUserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string PortalPassword { get; set; } = string.Empty;
    }
}
