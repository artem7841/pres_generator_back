using System.ComponentModel.DataAnnotations;


namespace PresentationCreator.Models;

public class EmailAnswer
{
    public string Message { get; set; }
    public int ExpiresIn { get; set; }
}

public class EmailRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256, MinimumLength = 3)]
    public string Email {get; set;}
}

public class EmailCodeLogin
{
    public string Email {get; set;}
    public string Code {get; set;}
}

public class JwtTokens
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
}

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Token is required")]
    public string RefreshToken {get; set;}
}

public class NewPresentation
{
    public string Id {get; set;}
    public byte[] Data {get; set;}
}

public class OrderRequest
{
    [Required(ErrorMessage = "Amount is required")]
    public string Amount {get; set;}
}