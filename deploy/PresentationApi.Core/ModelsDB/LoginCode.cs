namespace PresentationApi.ModelsBD;

public class LoginCode
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
}