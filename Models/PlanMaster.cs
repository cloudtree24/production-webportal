using System.ComponentModel.DataAnnotations;

namespace HCMPortal.Models
{
    public class PlanMaster
    {
        public int PlanId { get; set; }
        public string? PlanName { get; set; }

        public int? CreatedBy { get; set; } = 0;

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public int? UpdatedBy { get; set; } = 0;

        public DateTime? UpdatedDate { get; set; } = DateTime.Now;

        public int? IsDelete { get; set; } = 0;

        public int? Flag { get; set; } = 1;
        [Required(ErrorMessage = "Enter Plan Details")]
        public string? Remarks { get; set; }
        public int? NoofDays { get; set; }
    }
}
