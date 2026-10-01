using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HCMPortal.Models
{
    public partial class DynamicPage
    {
        public int PageId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200)]
        [DisplayName("Title")]
        public string Title { get; set; } = string.Empty;
        //to show the metatag--Jayanta(29/07/2026)
        public string? MetaTag { get; set; }
        [Required(ErrorMessage = "Slug is required.")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Invalid slug format.")]
        [DisplayName("Page URL")]
        [Remote("IsSlugExist", "DynamicPage", ErrorMessage = "URL already in use!Please try another one.", AdditionalFields = "OldSlug")]
        public string Slug { get; set; } = string.Empty;
        public string OldSlug { get; set; } = string.Empty;
        [DisplayName("Meta Title")]
        public string MetaTitle { get; set; } = string.Empty;
        [DisplayName("Meta Desription")]
        public string MetaDescription { get; set; } = string.Empty;
        public string MetaKeywords { get; set; } = string.Empty;

        [Required(ErrorMessage = "HTML Content is required.")]
        [DisplayName("Content")]
        public string HtmlContent { get; set; } = string.Empty;

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int IsDelete { get; set; }

        public int IsPublish { get; set; }
        public bool IsReadyToPublish { get; set; }
        public string BannerImagePath { get; set; } = string.Empty;
    }
}
