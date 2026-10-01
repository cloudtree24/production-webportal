namespace HCMPortal.ViewModel
{
    public class GSTResponseViewModel
    {
        public bool IsValid { get; set; }
        public string? GSTIN { get; set; }
        public string? LegalName { get; set; }
        public string? TradeName { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
    }
}
