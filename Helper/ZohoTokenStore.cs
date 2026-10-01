namespace HCMPortal.Helper
{
    public static class ZohoTokenStore
    {
        public static string ClientId { get; set; } = string.Empty;
        public static string ClientSecret { get; set; } = string.Empty;
        public static string AccessToken { get; set; } = string.Empty;
        public static string RefreshToken { get; set; } = string.Empty;
        public static string ApiDomain { get; set; } = string.Empty;
        public static DateTime ExpiryTime { get; set; }

        public static bool IsExpired => DateTime.Now >= ExpiryTime;
    }
}
