namespace HCMPortal.Models
{
    public partial class TblCompanySetUp
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

        public string AddressLine1 { get; set; } = null!;

        public string? AddressLine2 { get; set; }

        public string? Landmark { get; set; }

        public int CountryId { get; set; }

        public int StateId { get; set; }

        public string? StateCode { get; set; }

        public string City { get; set; } = null!;

        public string PostalCode { get; set; } = null!;

        public string? OrgRegdNo { get; set; }

        public string? Gstin { get; set; }

        public string? Pan { get; set; }

        public string? BaseCurrency { get; set; }

        public string? EmpIdprefix { get; set; }

        public string? SalarySlipIdprefix { get; set; }

        public string? PaymentSlipIdprefix { get; set; }

        public string? ProjectIdprefix { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public int IsDelete { get; set; }

        public int Flag { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
