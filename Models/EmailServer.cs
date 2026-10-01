namespace HCMPortal.Models
{
    public class EmailServer
    {
        public int Id { get; set; }
        public string? ConfigType { get; set; }
        public string? ServerName { get; set; }
        public string? BaseUrl { get; set; }
        public string? IncomingServer { get; set; }
        public string? OutgoingServer { get; set; }
        public long? Ssl { get; set; }
        public long? Tls { get; set; }
    }
}
