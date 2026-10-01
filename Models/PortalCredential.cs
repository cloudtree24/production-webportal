using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCMPortal.Models
{
    public class PortalCredential
    {
        public int Csid { get; set; }

        public string MembershipId { get; set; } = string.Empty;
        [Display(Name = "Business Name")]
        [Required(ErrorMessage = "Business Name is required")]
        public string CompanyName { get; set; } = string.Empty;
        [Display(Name = "Contact Person Name")]
        [Required(ErrorMessage = "Contact Person  is required")]
        public string ContactPersonName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email Address is required")]
        [Display(Name = "Email Address")]
        [Remote("IsEmailExist", "CustomerManagement", ErrorMessage = "Email Address is already in use!", AdditionalFields = "OldEmail")]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[a-zA-Z]{2,}(\.[a-zA-Z]{2,})?$", ErrorMessage = "Please enter a valid email address.")]
        public string? EmailAddress { get; set; }
        [Display(Name = "Phone Number")]
        [Remote("IsMobileNoExist", "CustomerManagement", ErrorMessage = "Phone number Already Exist", AdditionalFields = "OldPhone")]
        //[RegularExpression(@"^(?:[6-9]\d{9}|\+[1-9]\d{1,14})$", ErrorMessage = "Please enter phone number in E.164 format (e.g., +919876543210).")]
        [RegularExpression(@"^(?:[6-9]\d{9}|[1-9]\d{1,14})$",ErrorMessage = "Please enter phone number in E.164 format without '+' (e.g., 919876543210).")]
        public string? Phonenumber { get; set; }
        [Display(Name = "Industry")]
        public int? IndustryTypeId { get; set; }

        [Display(Name = "GSTIN")]
        //[Required(ErrorMessage = "GSTIN number is required")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$", ErrorMessage = "Please Enter a Valid GSTIN.")]
        public string? Gstinnumber { get; set; }
        [Display(Name = "PAN")]
        //[Required(ErrorMessage = "PAN number is required")]
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "Please Enter a Valid PAN.")]
        public string? Pancard { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }
        [Display(Name = "State")]
        public int? StateId { get; set; }
        [Display(Name = "Country")]
        public int? CountryId { get; set; }
        [Display(Name = "Postal Code")]
        [Required(ErrorMessage = "Postal Code is required")]
        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Please Enter a 6-digit Postal Code.")]
        public string? PostalCode { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; } = string.Empty;

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? IsDelete { get; set; }

        public string PortalUserName { get; set; } = string.Empty;

        public string PortalPassword { get; set; } = string.Empty;
        public string? CsProfileImage { get; set; }= string.Empty;

        public string AccountStatus { get; set; } = string.Empty;
        [NotMapped]
        public IFormFile? CsImageFile { get; set; }
    }
}

