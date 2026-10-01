namespace HCMPortal.ViewModel
{
    public class AmsnotificationTransactionViewModel
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string? SendMessage { get; set; }

        public bool? Active { get; set; }

        public int? SendBy { get; set; }

        public DateTime? SendDate { get; set; }

        public int? Flag { get; set; }

        public string? SenderEmail { get; set; }

        public string? ReceiverEmail { get; set; }
    }
}
