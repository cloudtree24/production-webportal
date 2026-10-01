using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace HCMPortal.Models
{
    public class TblHomeFeatureMaster
    {
        public int Hmid { get; set; }
        public string? Title { get; set; }
        public string? OldTitle { get; set; }
        public string? Content { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public int IsDelete { get; set; }
    }
}
