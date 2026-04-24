namespace PresentationApi.ModelsBD;

public class User
{
    public int Id { get; set; }                     
    public string Email { get; set; } = string.Empty; 
    public string Name { get; set; }          
    public string AvatarUrl { get; set; }     
    public string GoogleId { get; set; }     
    public string? FirebaseUid { get; set; }    
    public string Provider { get; set; } = "email"; 
    public DateTime CreatedAt { get; set; }         
    public bool HasActiveSubscription { get; set; } = false;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public string? YookassaPaymentMethodId { get; set; } 
    
    public List<LoginCode> LoginCodes { get; set; } = new();
}