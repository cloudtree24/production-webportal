namespace HCMPortal.ViewModel
{
    public class SeoConfigViewModel
    {
        public int Uscid { get; set; }

        public string MetaDescription { get; set; } = null!;

        public string MetaTitle { get; set; } = null!;

        public string? GoogleAnalyticsId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int IsActive { get; set; }
        public bool IsContentActive { get; set; }

        public int IsDelete { get; set; }

        public int Flag { get; set; }
        public string CreatedByName { get; set; } = String.Empty;
    }
}
