namespace HCMPortal.Models
{
    public class TblCrmcommunicationTransaction
    {
        public int TransactionId { get; set; }

        public string TransactionType { get; set; } = null!;

        public string QueryType { get; set; } = null!;

        public string MessageFrom { get; set; } = null!;

        public string MessageTo { get; set; } = null!;

        public string Subject { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string SendBy { get; set; } = null!;

        public DateTime SendDate { get; set; }

        public int IsDelete { get; set; }

        public string? Attachment { get; set; }
    }
}
