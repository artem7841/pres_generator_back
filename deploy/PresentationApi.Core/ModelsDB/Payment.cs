namespace PresentationApi.ModelsBD;

public class Payment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string YookassaPaymentId { get; set; } = string.Empty;
    public string Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    
    // Связь с пользователем
    public User User { get; set; } = null!;
}