namespace PresentationCreator.Models;

public class GoogleLoginRequest
{
    public string IdToken { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    public string GoogleId { get; set; }
}