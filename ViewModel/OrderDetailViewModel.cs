using HCMPortal.Models;

namespace HCMPortal.ViewModel
{
    public class OrderDetailViewModel
    {
        public string? OrderId { get; set; }
        public string? Status { get; set; }
        public string? RazorpayOrderId { get; set; }//for Razorpay order id Daitari(25-11-2025)

        //added for distingish the renewal of existing subscription--Anshuman 21.11.2025
        public string? RenewalMembershipId { get; set; }
        public float TaxPercentage { get; set; }
        public int planId { get; set; }
        public int Dmid { get; set; }
        public PlanMaster? planMaster { get; set; }
        public List<PlanMaster>? ListOfPlanMater { get; set; }
        public List<SubscriptionDetailsViewModel>? SubscriptionDetails { get; set; }
        public SubscriptionDetailsViewModel? SelectedSubscription { get; set; }
        public PortalCredentialViewModel? PortalCustomerDetails { get; set; }
        //added for payment response details save in DB--Anshuman 10.10.2025
        public PaymentResponse? paymentResponse { get; set; }
        public String? PaymentinitiatedBy { get; set; }
       

        public List<RazorpayCheckoutViewModel> Items { get; set; }
        public bool issubscriptionActive { get; set; } = false;
        //added for track any upgrade from the Trial Anshuman 07.02.2026
        public bool isupgradeFromTrial { get; set; } = false;
        public String? oldOrderId { get; set; }
        public int packCount { get; set; }
        public double packCost { get; set; }


      


    }


    public class RazorpayProcessModel
    {
        public OrderDetailViewModel? OrderDetail { get; set; }
        public OrderDetailViewModel? NewCompanyDetail { get; set; }
    }
}
