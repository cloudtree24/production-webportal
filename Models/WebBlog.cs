using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace HCMPortal.Models
{
    public class WebBlog
    {
        public int Wbid { get; set; }
        public string PostTitle { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImagePath { get; set; } = null!;

        public string WebSlugName { get; set; } = null!;

        public int ViewsCount { get; set; }

        public string BlogStatus { get; set; } = null!;
        public DateTime PostedDate { get; set; } = DateTime.Now;

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public int IsDelete { get; set; }

        public int Flag { get; set; }
    }
}
