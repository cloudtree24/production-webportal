using HCMPortal.Models;
using System.ComponentModel.DataAnnotations;
namespace HCMPortal.ViewModel
{
    public class ForgotpasswordViewModel
    {
        [Required(ErrorMessage = "Email Address is required")]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[a-zA-Z]{2,}(\.[a-zA-Z]{2,})?$", ErrorMessage = "Please enter a valid email address.")]
        public string? EmailAddress { get; set; }
       // public string? Password { get; set; }


    }
}
