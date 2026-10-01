namespace HCMPortal.Models
{
    public class TblSubscriptionMaster
    {
        public int Sid { get; set; }

        public string? SubscriptionName { get; set; }

        public int MaximumMemberCount { get; set; }

        public string? CompanyType { get; set; }

        public string Details { get; set; } = null!;

        public int IsActive { get; set; }

        public int IsDelete { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? IsRecommended { get; set; }

        public int? MaximumCompanyCount { get; set; }
    }
}
