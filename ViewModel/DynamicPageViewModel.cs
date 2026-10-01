using HCMPortal.Models;

namespace HCMPortal.ViewModel
{
    public class DynamicPageViewModel : DynamicPage
    {
        //Types--General(No extra view),Blog(dynamic as per the Blog page)
        //if need more add those here
        public string PageType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int ViewsCount { get; set; } 
        public int? LikeCount { get; set; } //To add Like Count--Jayanta(10/07/2026)
        public List<BlogViewModel>? relatedBlogs { get; set; }
    }
}
