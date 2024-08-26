namespace sociosphere.Models
{
    public class RazorpaySettings
    {
        public string KeyId { get; set; }
        public string KeySecret { get; set; }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
    }
}
