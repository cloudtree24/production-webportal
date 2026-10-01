using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HCMPortal.Helper
{
    public static class GlobalProperty
    {
        public static string MetaTitle { get; set; } = string.Empty;
        public static string MetaDescription { get; set; } = string.Empty;
        public static string GoogleAnalyticsId { get; set; } = string.Empty;
    }
}
