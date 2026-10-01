namespace HCMPortal.ViewModel
{
    public partial class GalleryMasterViewModel
    {
        public int GalleryId { get; set; }
        public string Title { get; set; } = null!;
        public string? AttachmentPath { get; set; }
        public string? Description { get; set; }
        public bool Status { get; set; } = true;
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int IsDelete { get; set; }
        public bool Flag { get; set; } = true;
    }
}
