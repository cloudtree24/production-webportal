namespace HCMPortal.ViewModel
{
    public class SubscriptionDetailsViewModel
    {
        public int Sbfid { get; set; }

        public int Sid { get; set; }
        public string? SubscriptionName { get; set; }

        public int MaximumMemberCount { get; set; }

        public string? CompanyType { get; set; }

        public string? Details { get; set; }

        public int IsActive { get; set; }

        public int Bid { get; set; }

        public int Amount { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public int? IsDelete { get; set; }
        public int? IsRecommended { get; set; }
        public int? MaximumCompanyCount { get; set; }
        //added for distingish the renewal of existing subscription--Anshuman 21.11.2025
        public string? ActiveMembershipId { get; set; }
        public int? ValidUptoDays { get; set; }
        //added to get the addon details if exist--Anshuman 30.01.2026
        public int? PerpackCount { get; set; }
        public decimal? PackCost { get; set; }

        public int? NoofDays { get; set; }
    }
}
