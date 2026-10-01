using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPortal.Models
{
    public class TblCandiateApplication
    {
        public int Cid { get; set; }

        public int Jid { get; set; }
        [Required(ErrorMessage = "Full Name is required")]
        public string Name { get; set; } = null!;
        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter valid 10 digit mobile number")]
        public string Mobile { get; set; } = null!;
        [Required(ErrorMessage = "Email Address is required")]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[a-zA-Z]{2,}(\.[a-zA-Z]{2,})?$", ErrorMessage = "Please enter a valid email address.")]
        public string EmailAddress { get; set; } = null!;

        public string? Cvupload { get; set; }

        public string? Remarks { get; set; }

        public string? Status { get; set; }

        public int IsDelete { get; set; }

        public int FlagS { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
        [NotMapped]
        [Required(ErrorMessage = "Please upload resume")]
        public IFormFile? ResumeFile { get; set;}
    }
}
