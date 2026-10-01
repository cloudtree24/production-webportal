namespace HCMPortal.Models
{
    //for  Razorpay payment details storage Daitari(25-11-2025)
    public class Payment_Master
    {
        public int PaymentId { get; set; }

        public int Csid { get; set; }

        public string RazorpayOrderId { get; set; } = string.Empty;

        public string? RazorpayPaymentId { get; set; }

        public string? RazorpaySignature { get; set; }

        public double Amount { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Status { get; set; } = "Created";
        // Created, Success, Failed, Pending

        public string? PaymentResponseJson { get; set; } // store full return data

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }
    }
}
