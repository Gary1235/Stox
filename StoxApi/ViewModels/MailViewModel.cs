// SMTP 伺服器設定
public class SmtpConfig
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587; // 通常 TLS 為 587, SSL 為 465
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string SenderEmail { get; set; } = "";
}

// 信件內容載體
public class EmailMessage
{
    public string To { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsHtml { get; set; } = true;

    // 選擇性欄位 (選填)
    public List<string> Cc { get; set; } = new List<string>();
    public List<string> Bcc { get; set; } = new List<string>();
    public List<string> AttachmentPaths { get; set; } = new List<string>();
}