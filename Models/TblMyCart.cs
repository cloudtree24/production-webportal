namespace HCMPortal.Models
{
    public class TblMyCart
    {
        public int CartId { get; set; }

        public int UserId { get; set; }

        public string OrderNo { get; set; } = null!;

        public int Sid { get; set; }

        public int Pid { get; set; }

        public string? TransactionStatus { get; set; }

        public string? OrderStatus { get; set; }

        public int IsDelete { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }
    }
}
