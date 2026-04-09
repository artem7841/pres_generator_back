namespace PresentationCreator.Models;

public class RefreshTokenAnswer
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public int Expires { get; set; }
}