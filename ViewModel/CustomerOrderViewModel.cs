using HCMPortal.Models;

namespace HCMPortal.ViewModel
{
    public class CustomerOrderViewModel:PortalCredential
    {
        public double TotalAmount { get; set; }
        public string? OrderType { get; set; }
        public string?  OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string?  InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public PaymentResponse? paymentResponse { get; set; }
    }
    public class CustomerOrderDetails : CustomerOrderDetail
    {
        public string? SubscriptionName { get; set; }
        public string? BillingName { get; set; }
        //for distinguish between the Subscription and Add-on service type
        public string? RecordType { get; set; }
        public decimal? Discountamount { get; set; }
        public int SubscriptionRenewalId { get; set; }
    }
    public partial class CustomerOrderDetail
    {
        public int CsorderId { get; set; }
        public string? InvoiceId { get; set; }

        public string? OrderId { get; set; }

        public int? Csid { get; set; }

        public int SubscriptionBillingFrequencyId { get; set; }

        public decimal? SubscriptionAmount { get; set; }

        public int? DiscountId { get; set; }

        public int? AppliedTaxId { get; set; }

        public decimal? TaxAmount { get; set; }

        public decimal? NetAmount { get; set; }

        public string? BillingAddress { get; set; }

        public string? OrderStatus { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? CreatedBy { get; set; }

        public string? ModifiedBy { get; set; }

        public string? TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public string? TransactionStatus { get; set; }

        public string? OrderType { get; set; }

        public DateTime? PurchasedDate { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
    }
}
