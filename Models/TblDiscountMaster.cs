namespace HCMPortal.Models
{
    public class TblDiscountMaster
    {
        public int Dmid { get; set; }

        public string DiscountName { get; set; } = null!;

        public string DiscountCode { get; set; } = null!;

        public string DiscountType { get; set; } = null!;

        public string DiscountValue { get; set; } = null!;

        public DateTime AvailableFromDate { get; set; }

        public DateTime AvailableTo { get; set; }

        public string? LifecycleType { get; set; }

        public string? Details { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? IsDelete { get; set; }
    }
}
