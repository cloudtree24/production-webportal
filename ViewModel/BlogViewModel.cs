using HCMPortal.Models;

namespace HCMPortal.ViewModel
{
    public class BlogViewModel:WebBlog
    {
        public string CreatedByName { get; set; } = string.Empty;
        public List<BlogViewModel> Blogs { get; set; } = new();

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    }
    public class BlogListViewModel
    {
        public List<BlogViewModel>? Blogs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}
