namespace AutoCarShowroom.Models
{
    public class EmailOptions
    {
        public bool Enabled { get; set; }

        public string SmtpHost { get; set; } = string.Empty;

        public int SmtpPort { get; set; } = 587;

        public bool EnableSsl { get; set; } = true;

        public string SenderEmail { get; set; } = string.Empty;

        public string SenderName { get; set; } = "AutoCarShowroom";

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
