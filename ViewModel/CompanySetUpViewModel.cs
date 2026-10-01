using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HCMPortal.ViewModel
{
    public class CompanySetUpViewModel
    {
        public int CompanyId { get; set; }

        public string OrgName { get; set; } = null!;

        public string? LogoUrl { get; set; }
        public string? ContactPerson { get; set; }
        public string PrimaryContact { get; set; } = null!;

        public string? AltContact { get; set; }
        public string PrimaryEmail { get; set; } = null!;

        public string? AltEmail { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? PinterestUrl { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? Landmark { get; set; }

        public int CountryId { get; set; }

        public int StateId { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? OrgRegdNo { get; set; }
        public string? Gstin { get; set; }
        public string? Pan { get; set; }
        public string? BaseCurrency { get; set; }
        public string? EmpIdprefix { get; set; }
        public string? SalarySlipIdprefix { get; set; }
        public string? PaymentSlipIdprefix { get; set; }
        public string? ProjectIdprefix { get; set; }


    }
}
