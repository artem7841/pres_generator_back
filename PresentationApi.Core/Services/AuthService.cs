using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PresentationApi.ModelsBD;
using PresentationCreator.interfaces;
using PresentationCreator.Models;

namespace PresentationCreator;

public class AuthService : IAuthService
{
    private IEmailService _emailService;
    private SmtpSettings _settings;
    private Random _random;
    private ICodeRepo _codeRepo;
    private readonly AuthOptions _authOptions;
    private IUserRepo _userRepo;
    private IJwtGenerator _jwtGenerator;

    public AuthService(IOptions<AuthOptions> authOptions, IEmailService emailService, 
        IOptions<SmtpSettings> settings, Random random, ICodeRepo codeRepo, IUserRepo userRepo,  IJwtGenerator jwtGenerator)
    {
        _authOptions = authOptions.Value;
        _emailService = emailService;
        _settings = settings.Value;
        _random = random;
        _codeRepo = codeRepo;
        _userRepo =  userRepo;
    }

    public async Task<User> GetUser(string id)
    {
        int.TryParse(id, out int userId);
        return await _userRepo.GetUserById(userId);
    }

    public async Task<EmailAnswer> SendCodeOnEmail(string email)
    {
        EmailAnswer answer = new EmailAnswer();
        
        if (string.IsNullOrWhiteSpace(email))
        {
            answer.Message = "Email address is required";
            answer.ExpiresIn = 0;
            return answer;
        }
        
        try
        {
            string code = _random.Next(1000, 9999).ToString();
            await _emailService.SendCodeAsync(email, code);
            answer.Message = "Success";
            answer.ExpiresIn = 300;
        }
        catch (Exception ex)
        {
            answer.Message = ex.Message;
            return  answer;
        }

        return answer;
    }
    
    public async Task<JwtTokens> ApproveCode(string email, string code)
    {
        var lastCode = await _codeRepo.GetLastCode(email);
        if (lastCode == null)
        {
            throw new Exception("Код не найден или не был отправлен");
        }
        if (DateTime.Now < lastCode.ExpiresAt && code == lastCode.Code)
        {
            User user = new User()
            {
                Email = email,
                AvatarUrl = null,
                CreatedAt = DateTime.Now,
                FirebaseUid = null,
                GoogleId = null,
                HasActiveSubscription = false,
                Name = email
            };
            
            User newUser = await _userRepo.AddUserIfNotExist(user);
            
            var jwt = _jwtGenerator.GenerateJwtToken(newUser);
            
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            var refreshToken = GenerateRefreshToken();

            return new JwtTokens
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }
        throw new Exception("Code expired or invalid");
    }

    public Task<RefreshTokenAnswer> RefreshToken(string refreshToken)
    {
        throw new NotImplementedException();
    }

    public Task Logout(string token)
    {
        throw new NotImplementedException();
    }
    
    
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}