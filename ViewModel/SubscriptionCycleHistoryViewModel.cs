namespace HCMPortal.ViewModel
{
    public class SubscriptionCycleHistoryViewModel
    {
        public string? RazorpaySubscriptionId { get; set; }
        public string? PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public int CycleNo { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Status { get; set; }
        public string? OrderID { get; set; }
    }
}
