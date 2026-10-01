namespace HCMPortal.Models
{
    public class TblJobPostDetail
    {
        public int Jid { get; set; }

        public string JobTitle { get; set; } = null!;

        public string JobDescription { get; set; } = null!;

        public string JobType { get; set; } = null!;

        public string Status { get; set; } = null!;

        public int IsDelete { get; set; }

        public int FlagS { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
