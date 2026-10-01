using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPortal.ViewModel
{
    public class SupportPageViewModel:TicketMaster
    {
        [NotMapped]
        public List<TicketMaster>? ticketList { get; set; }
        //thi is for getting the dynamic product list as per the customer purchase
        //[NotMapped]
        //public List<SubscriptionDetailsViewModel>? subscriptionDetails { get; set; }
        public int isToBeActive { get; set; }
        public IFormFile? attachment { get; set; }
        public DateTime? FromDate { get; set; } 
        public DateTime ToDate { get; set; } = DateTime.Now.Date;
        public string? Category { get; set; }
        public string? ZohoTicketId { get; set; }
        public string? ZohoAttachMentId { get; set; }

    }
    public partial class TicketMaster
    {
        public int Tmid { get; set; }

        public int TicketId { get; set; }

        public int CustomerId { get; set; }

        public int? ProductId { get; set; }

        public int? AssigneeId { get; set; }

        public string? QueryType { get; set; }

        public string? PriorityType { get; set; }
        public string? AttachedFileName { get; set; }

        public string? Subject { get; set; }

        public string? Summary { get; set; }

        public string? ResolveStatus { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? RespondDate { get; set; }

        public string? RespondBy { get; set; }

        public int? ClosedBy { get; set; }

        public DateTime? ClosedDate { get; set; }
        public DateTime? TimeOfIssue { get; set; }
    }
}
