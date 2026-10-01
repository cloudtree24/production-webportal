using HCMPortal.Models;
using Microsoft.Build.Evaluation;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPortal.ViewModel
{
    public class CustomerSubscriptionDetailsViewModel : PortalCredential
    {
        public int companyId { get; set; }
        public string? StateName { get; set; }
        public string? OrderType { get; set; }
        public string? CountryName { get; set; }
        public string? OrderId { get; set; }
        public int sid { get; set; }
        //to get the company type like group or individual-Jayanta(18-12-2025)
        public string? CompanyType { get; set; }=string.Empty;
        public string? SusbcriptionName { get; set; }
        public string? SusbcriptionStatus { get; set; }
        public int bid { get; set; }
        public string? BillingName { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime NextrenewalDate { get; set; }
        public DateTime PurchaseDate { get; set; }

        public string? InvoiceNumber { get; set; }
        public double SubscriptionBaseAmount { get; set; }
        public int MaximumCompanyCreationAllowed { get; set; }
        public int MaximumEmployeCreationAllowed { get; set; }
        public double SubscriptionAmount { get; set; }
        public string? SubscriptionDetails { get; set; }
        public int CsorderId { get; set; }
        public int SubscriptionRenewalId { get; set; }

        public string? CancelReason { get; set; }
        public int PerPackCount { get; set; }
        public decimal PackCost { get; set; }
        public int PurchasedAddonPackCount { get; set; }
        public int PlanDays { get; set; }
        public decimal OriginalAddonAmount { get; set; }
        //for purchased addon user count details
        [NotMapped]
        public bool IsRequestForCancelation { get; set; }
        [NotMapped]
        public bool IsRequestAccepted { get; set; }
        [NotMapped]
        public bool IsRequestRejected { get; set; }

    }
}
