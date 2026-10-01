namespace HCMPortal.ViewModel
{
    public class RazorpayDiscountIdViewModel
    {
        public int RazorpayDiscountId { get; set; }

        public int? CsId { get; set; }

        public int? Dmid { get; set; }

        public string DiscountCode { get; set; } = string.Empty;

        public string RazorpayOfferId { get; set; } = string.Empty;

        public string? RazorpaySubscriptionId { get; set; } = string.Empty;

        public string? PlanId { get; set; } = string.Empty;

        public string? TransactionId { get; set; } = string.Empty;

        public string? Status { get; set; } = string.Empty;

        public DateTime? AvailableFromDate { get; set; }

        public DateTime? AvailableTo { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? IsDelete { get; set; }
    }
}
