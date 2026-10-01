namespace HCMPortal.Models
{
    public class EmailModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; } = 0;
        public string? Name { get; set; }
        public string? Company { get; set; }
        public string? toEmail { get; set; }
        public string? fromMail { get; set; }
        public string Title { get; set; } = null!;
        public string? Subject { get; set; }
       
    }
}
