namespace HCMPortal.Models
{
    public class PayPhiPaymentRequest
    {
        public string merchantId { get; set; }
        public string merchantTxnNo { get; set; }
        public string amount { get; set; }
        public string txnDate { get; set; }
        public string customerEmailID { get; set; }
        public string customerMobileNo { get; set; }
        public string returnURL { get; set; }
        public string addlParam1 { get; set; }
        public string addlParam2 { get; set; }
        public string secureHash { get; set; }
        // Fixed parameters required by PayPhi API, typically not variable
        public string currencyCode { get; set; } = "356";
        public string payType { get; set; } = "1";
        public string paymentMode { get; set; } = "CARD";
        public string transactionType { get; set; } = "SALE";
        // PayPhi API URL (QA/Test environment)
        public string PayPhiGatewayUrl { get; set; } = "https://qa.phicommerce.com/pg/api/v2/initiateSale";
    }
}
