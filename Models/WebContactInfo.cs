using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace HCMPortal.Models
{
    public class WebContactInfo
    {
        public int Wcid { get; set; }

        public string Title { get; set; } = null!;

        public string AddressDetail { get; set; } = null!;
        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string Status { get; set; } = null!;

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int IsDelete { get; set; }

        public int Flag { get; set; }

        public string? ZExtra1 { get; set; }

        public string? ZExtra2 { get; set; }
    }
}
