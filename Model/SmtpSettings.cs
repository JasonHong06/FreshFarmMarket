public class SmtpSettings
{
    public required string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public required string Username { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
    public bool EnableSSL { get; set; }
    public bool UseSSL { get; set; } // ✅ Add this line
}
