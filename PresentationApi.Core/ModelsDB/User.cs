namespace PresentationApi.ModelsBD;

public class User
{
    public int Id { get; set; }                      // Первичный ключ
    public string Email { get; set; } = string.Empty; // Email пользователя
    public DateTime CreatedAt { get; set; }          // Дата регистрации
    public bool HasActiveSubscription { get; set; } = false; // Статус подписки
    public DateTime? SubscriptionExpiresAt { get; set; } // Когда истекает
    public string? YookassaPaymentMethodId { get; set; } // ID способа оплаты
    
    // Связь с таблицей кодов (один пользователь → много кодов)
    public List<LoginCode> LoginCodes { get; set; } = new();
}