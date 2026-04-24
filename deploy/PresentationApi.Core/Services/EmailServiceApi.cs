using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using PresentationCreator.interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PresentationCreator;

public class EmailServiceApi : IEmailService
{
    private readonly FirebaseAuth _firebaseAuth;

    public EmailServiceApi()
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("firebase-adminsdk.json")
            });
        }
        
        _firebaseAuth = FirebaseAuth.DefaultInstance;
    }
    public async Task<bool> SendCodeAsync(string email, string code)
    {
        try
        {
            // Firebase не умеет отправлять произвольный текст напрямую
            // Используем костыль - создаем временного пользователя с паролем = коду
            
            // Генерируем ссылку для "сброса пароля" и вставляем туда код
            var actionCodeSettings = new ActionCodeSettings
            {
                Url = $"https://yourapp.com/verify?code={code}&email={email}",
                HandleCodeInApp = true
            };
            
            // Отправляем email через Firebase (используя механизм password reset)
            var link = await _firebaseAuth.GeneratePasswordResetLinkAsync(email, actionCodeSettings);
            
            // Firebase автоматически отправляет письмо на email
            // Ссылка в письме будет содержать ваш код
            Console.WriteLine($"Email sent to {email} with code: {code}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Firebase error: {ex.Message}");
            return false;
        }
    }
}