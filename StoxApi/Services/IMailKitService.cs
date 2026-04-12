using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public interface IMailKitService
{
    /// <summary>
    /// 發信(共用)
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public Task<bool> SendEmailAsync(EmailMessage email);
}

public class MailKitService : IMailKitService
{
    private readonly SmtpConfig _config;

    // 如果你有用 DI，可以把 SmtpConfig 透過建構子注入
    public MailKitService(SmtpConfig config)
    {
        _config = config;
    }

    public async Task<bool> SendEmailAsync(EmailMessage email)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_config.SenderName, _config.SenderEmail));
            message.To.Add(MailboxAddress.Parse(email.To));

            // 處理副本 (CC)
            foreach (var cc in email.Cc)
                message.Cc.Add(MailboxAddress.Parse(cc));

            // 處理密件副本 (BCC)
            foreach (var bcc in email.Bcc)
                message.Bcc.Add(MailboxAddress.Parse(bcc));

            message.Subject = email.Subject;

            // 建立信件內容與附件
            var bodyBuilder = new BodyBuilder();
            if (email.IsHtml)
                bodyBuilder.HtmlBody = email.Body;
            else
                bodyBuilder.TextBody = email.Body;

            // 處理附件
            foreach (var filePath in email.AttachmentPaths)
            {
                if (File.Exists(filePath))
                {
                    await bodyBuilder.Attachments.AddAsync(filePath);
                }
            }

            message.Body = bodyBuilder.ToMessageBody();

            // 執行發信
            using var client = new SmtpClient();
            // 忽略憑證錯誤 (如果是在內部測試環境常遇到憑證問題可開啟，正式環境建議拿掉)
            // client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(_config.Host, _config.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config.Username, _config.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
        catch (Exception ex)
        {
            // 這裡可以換成你的 NLog / Serilog 等 Log 紀錄
            Console.WriteLine($"發信失敗: {ex.Message}");
            return false;
        }
    }
}