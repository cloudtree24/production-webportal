

using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPortal.Models
{
    public class TblExecutiveDetail
    {
        public int Edid { get; set; }

        public string Name { get; set; } = null!;

        public string? ExecutiveImage { get; set; } = null!;

        public string Designation { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Status { get; set; } = null!;

        public int IsDelete { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public int FlagS { get; set; }
        [NotMapped]
        public IFormFile? CsImageFile { get; set; }
    }
}
