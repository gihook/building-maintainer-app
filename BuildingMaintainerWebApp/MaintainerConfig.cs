namespace BuildingMaintainerWebApp;

public class MaintainerConfig
{
    public string CredentialsPath { get; set; } = string.Empty;
    public string SpreadsheetId { get; set; } = string.Empty;
    public string RangeEmails { get; set; } = string.Empty;
    public string RangeReplacer { get; set; } = string.Empty;
    public string RangeHistory { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPass { get; set; } = string.Empty;
}