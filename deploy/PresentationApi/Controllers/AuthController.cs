using System.IdentityModel.Tokens.Jwt;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PresentationCreator.interfaces;
using PresentationCreator.Models;

namespace PresentationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private IJwtGenerator _jwtGenerator;
    private IGoogleService _googleService;

    public AuthController(IGoogleService googleService, IJwtGenerator jwtGenerator)
    {
        _googleService = googleService;
        _jwtGenerator = jwtGenerator;
    }
    
    [HttpPost("google")]
    public async Task<IResult> Google([FromBody] GoogleAuthRequest request)
    {
        Console.WriteLine($"Google Auth Request: {request}");
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { Environment.GetEnvironmentVariable("GOOGLE_ID") }
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        
            var user = await _googleService.AuthenticateWithGoogleAsync(
                payload.Subject,
                payload.Email,
                payload.Name,
                payload.Picture
            );
        
            var token = _jwtGenerator.GenerateJwtToken(user);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine(token);
            return Results.Ok(new { tokenString, user });
        }
        catch (DbUpdateException ex)
        {
            var innerMessage = ex.InnerException?.Message ?? "No inner exception";
            var innerInnerMessage = ex.InnerException?.InnerException?.Message ?? "";
        
            Console.WriteLine($"DB ERROR: {ex.Message}");
            Console.WriteLine($"INNER: {innerMessage}");
            Console.WriteLine($"INNER INNER: {innerInnerMessage}");
        
            return Results.BadRequest(new { 
                error = $"Database error: {innerMessage}",
                details = innerInnerMessage
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = $"Google auth failed: {ex.Message}" });
        }
    }
}