namespace HCMPortal.Models
{
    public class TblSlotMaster
    {
        public int Slid { get; set; }

        public string Day { get; set; } = null!;

        public string SlotName { get; set; } = null!;

        public TimeOnly? SlotStartTime { get; set; }

        public TimeOnly? SlotEndTime { get; set; }

        public int IsDelete { get; set; }

        public int? FlagS { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
