namespace HCMPortal.Models
{
    public class TrainingModule
    {
        public int Tmid { get; set; }

        public string Title { get; set; } = null!;

        public int FeatureMenuId { get; set; }

        public string FeatureDetails { get; set; } = null!;

        public int? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? ModfiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int IsDelete { get; set; }
    }
}
