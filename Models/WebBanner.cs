using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPortal.Models
{
    public class WebBanner
    {
        public int Wbid { get; set; }
        public string ImageName { get; set; } = null!;

        public string TextContent { get; set; } = null!;

        public int? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int? ModfiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int IsDelete { get; set; }

        public int IsActive { get; set; }
        /// <summary>
        /// Home or product
        /// </summary>
        public string? PageFor { get; set; }
    }
}
