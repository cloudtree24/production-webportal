namespace HCMPortal.ViewModel
{
    public class CancelSubscriptionDetailsViewModel
    {
        public int Scid { get; set; }

        public string MembershipId { get; set; } = null!;

        public string CancelReason { get; set; } = null!;

        public int IsDetele { get; set; }

        public int Flag { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public string? ResponseReason { get; set; }
     

        public int? ResponsedBy { get; set; }
        public int Csid { get; set; }
    }
}
