namespace HCMPortal.Models
{
    //for Razorpay checkout view model Daitari(25-11-2025)
    public class RazorpayCheckoutViewModel
    {
        public string Key { get; set; }
        public string OrderId { get; set; }
        public double Amount { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CustomerName { get; set; }
        public string JsonData { get; set; }
        public int Csid { get; set; }
        public string EncryptedAmount { get; set; }
        public string SubscriptionId { get; set; }
        public bool IsSubscription { get; set; }

        public bool IsAddonPayment { get; set; }
        public bool isUpgrade { get; set; }

    }
}
