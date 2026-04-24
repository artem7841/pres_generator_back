using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using PresentationCreator.interfaces;
using PresentationCreator.Models;


namespace PresentationCreator;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private ICodeRepo _codeRepo;
    
    public EmailService(IOptions<SmtpSettings> options,  ICodeRepo codeRepo)
    {
        _codeRepo = codeRepo;
        _settings = options.Value;
    }
    
    public async Task<bool> SendCodeAsync(string email, string code)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email получателя пустой");
        }
        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port);
            client.EnableSsl = _settings.EnableSsl;
            
            if (!string.IsNullOrEmpty(_settings.Username))
            {
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }
            
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            
            var mail = new MailMessage
            {
                From = new MailAddress(_settings.From, _settings.FromName),
                Subject = "Ваш код подтверждения",
                Body = $@"
                    <html>
                    <body>
                        <h2>Код подтверждения</h2>
                        <p>Ваш код: <strong>{code}</strong></p>
                        <p>Код действителен 5 минут.</p>
                    </body>
                    </html>",
                IsBodyHtml = true
            };
            
            mail.To.Add(email);
            
            await client.SendMailAsync(mail);
            await _codeRepo.AddCode(code, email);
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}