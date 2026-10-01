using HCMPortal.Models;

namespace HCMPortal.ViewModel
{
    public class AccountDeleteViewModel 
    {
        public string ContactPersonName { get; set; } = null!;

        public string? EmailAddress { get; set; }

        public string? Phonenumber { get; set; }
        public int Did { get; set; }

        public int CustomerId { get; set; }

        public DateTime RequestedDate { get; set; }

        public string Reason { get; set; } = null!;

        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? ModidiedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public int IsDelete { get; set; }
    }
}
