namespace HCMPortal.ViewModel
{
    public class AddOnPaymentViewModel
    {
        public string MembershipId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public int NumberOfPack { get; set; }
        public int totalUsers { get; set; }
        public decimal PackCost { get; set; }
        public decimal TaxAmount { get; set; } 
        public decimal NetAmount { get; set; }
        public string BillingAddress { get; set; } = string.Empty;
        public string ContactPersonName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TransactionStatus { get; set; } = string.Empty;
        public string? TransactionId { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
    }
}
