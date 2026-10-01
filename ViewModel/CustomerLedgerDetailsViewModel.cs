using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using HCMPortal.Models;

namespace HCMPortal.ViewModel
{
    public class GenerateInvoiceViewModel : TblCustomerOrderDetail
    {
        public double DiscountAmount { get; set; }
        [NotMapped]
        public string? Base64LogoString { get; set; }
        public string? ProductName { get; set; }
        public CustomerLedgerDetailsViewModel? customerDetails { get; set; }
        public TblTaxMaster? taxDetails { get; set; }
        public SelfMasterDetailsViewModel? SelfDetails { get; set; }
    }
    public class CustomerLedgerDetailsViewModel : PortalCredential
    {
        public int companyId { get; set; }
        public string? StateName { get; set; }
        public string? CountryName { get; set; }
        public string? SusbcriptionName { get; set; }
        public string? SusbcriptionStatus { get; set; }
        public string? BillingName { get; set; }
        public string? InvoiceNumber { get; set; }


        public SelfMasterDetailsViewModel? SelfDetails { get; set; }
    }
    public class SelfMasterDetailsViewModel : SelfMasterDetails
    {
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IFSCCode { get; set; }
        public string? StateName { get; set; }
        public int? StateCode { get; set; }
        public string? CountryName { get; set; }


    }
    public class SelfMasterDetails
    {
        public int CompanyId { get; set; }

        public string? OrgName { get; set; }

        public string? LogoUrl { get; set; }
        public string? ContactPerson { get; set; }
        public string? PrimaryContact { get; set; }
        public string? AltContact { get; set; }
        public string? PrimaryEmail { get; set; }
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
        public int? IsDelete { get; set; }
        public int? Flag { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? Tannumber { get; set; }
    }
    public partial class TblCustomerOrderDetail
    {
        public int CsorderId { get; set; }

        public int? Csid { get; set; }

        public string? InvoiceId { get; set; }

        public string? OrderId { get; set; }

        public int SubscriptionBillingFrequencyId { get; set; }

        public decimal? SubscriptionAmount { get; set; }

        public int? DiscountId { get; set; }

        public int? AppliedTaxId { get; set; }

        public decimal? TaxAmount { get; set; }

        public decimal? NetAmount { get; set; }

        public string? BillingAddress { get; set; }

        public string? OrderStatus { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string CreatedBy { get; set; } = null!;

        public string? ModifiedBy { get; set; }

        public string? TransactionId { get; set; }

        public DateTime? TransactionDate { get; set; }

        public string? TransactionStatus { get; set; }

        public string? OrderType { get; set; }

        public DateTime? PurchasedDate { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }

        public string? MembershipId { get; set; }
    }


    public class InvoiceMasterViewModel
    {
        public int InvoiceId { get; set; }
        public int OrderId { get; set; }
        public int CsId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? InvoiceStatus { get; set; }
        public int CreatedBy { get; set; }
        public List<InvoiceItem>? invoiceItems { get; set; }

    }
    public class InvoiceItem
    {
        public int Id { get; set; }
        public int Qty { get; set; }

        public string ServiceName { get; set; } = null!;

        public string Hsncode { get; set; } = null!;

        public decimal ServiceAmount { get; set; }

        public decimal? DiscountPercentage { get; set; }

        public decimal? Igstamount { get; set; }

        public decimal? Cgstamount { get; set; }

        public decimal? Sgstamount { get; set; }

        public decimal? Utgstamount { get; set; }

        public decimal NetAmount { get; set; }




    }

    public class InvoicePrintDetailsViewModel : InvoiceMasterViewModel
    {
        public CustomerLedgerDetailsViewModel? customerDetails { get; set; }
        public string? OrderNumber { get; set; }
        public string? RecordType { get; set; }
        public SelfMasterDetailsViewModel? SelfDetails { get; set; }
        [NotMapped]
        public string? Base64LogoString { get; set; }
        [NotMapped]
        public string? QrCodeUrl { get; set; }
        [NotMapped]
        public string? QrCodeBase64Image { get; set; }
    }
}
