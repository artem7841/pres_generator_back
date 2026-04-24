using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotNetEnv;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PresentationApi;
using PresentationApi.Data;
using PresentationApi.Infrastructure.repositories;
using PresentationCreator;
using PresentationCreator.interfaces;
using PresentationCreator.Models;

var solutionRoot = FindSolutionRoot();
var envPath = Path.Combine(solutionRoot, ".env");
        
if (File.Exists(envPath))
{
    Env.Load(envPath);
}
else
{
    Console.WriteLine($"Warning: .env not found at {envPath}");
    Env.Load();
}
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("https://prezaai.ru", "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("X-Presentation-Id");;
        });
});


var authOptions = new AuthOptions
{
    Issuer = builder.Configuration["AuthOptions:Issuer"] ?? "PresentationCreator",
    Audience = builder.Configuration["AuthOptions:Audience"] ?? "PresentationCreatorClient",
    SecretKey = Environment.GetEnvironmentVariable("AUTH_SECRET_KEY") 
                ?? throw new InvalidOperationException("AUTH_SECRET_KEY is missing"),
    TokenLifetime = 21600
};

builder.Services.Configure<AuthOptions>(opts =>
{
    opts.Issuer = authOptions.Issuer;
    opts.Audience = authOptions.Audience;
    opts.SecretKey = authOptions.SecretKey;
    opts.TokenLifetime = authOptions.TokenLifetime;
});


builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = authOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SecretKey)),
        };
    });

var smtpSettings = new SmtpSettings
{
    Host = builder.Configuration["Smtp:Host"] ?? "smtp.yandex.ru",
    Port = int.Parse(builder.Configuration["Smtp:Port"] ?? "587"),
    Username = Environment.GetEnvironmentVariable("SMTP_USERNAME") 
               ?? throw new InvalidOperationException("SMTP_USERNAME is missing"),
    Password = Environment.GetEnvironmentVariable("PASSWORD_YANDEX_MAIL") 
               ?? throw new InvalidOperationException("PASSWORD_YANDEX_MAIL is missing"),
    From = Environment.GetEnvironmentVariable("SMTP_FROM") 
           ?? throw new InvalidOperationException("SMTP_FROM is missing"),
    FromName = builder.Configuration["Smtp:FromName"] ?? "Authentication Service",
    EnableSsl = bool.Parse(builder.Configuration["Smtp:EnableSsl"] ?? "true")
};

builder.Services.Configure<SmtpSettings>(opts =>
{
    opts.Host = smtpSettings.Host;
    opts.Port = smtpSettings.Port;
    opts.Username = smtpSettings.Username;
    opts.Password = smtpSettings.Password;
    opts.From = smtpSettings.From;
    opts.FromName = smtpSettings.FromName;
    opts.EnableSsl = smtpSettings.EnableSsl;
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();
builder.Services.AddScoped<IService, Service>();
builder.Services.AddScoped<Random>();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailService, EmailServiceApi>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICodeRepo, CodeRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IFileRepo, FileRepo>();
builder.Services.AddScoped<IGoogleService, GoogleService>();
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAiHandler, AiHandler>();
builder.Services.AddHttpClient();
builder.Services.AddTransient<IPptxToPdfConverter, PptxToPdfConverter>();
builder.Services.AddScoped<ISlideController, SlideController>();

builder.Services.AddScoped<YandexImageSearchService>();
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<IPaymentRepo, PaymentRepo>();
builder.Services.AddScoped<IPaymentCreator, UKassaPaymentCreator>();
builder.Services.AddControllers();

builder.Services.AddStackExchangeRedisCache(options =>
{

    options.Configuration = "localhost:6379";
});

builder.Services.AddScoped<IImageCache, ImageCache>();

var app = builder.Build();

app.UseExceptionHandler(); 
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers(); 

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.Migrate();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/api/webhook", async (
    IPaymentCreator paymentCreator,
    [FromBody] YooKassaWebhook payload) =>
{
    Console.WriteLine("Webhook " + payload);

    if (payload.Event == "payment.succeeded")
    {
        try
        {
            var paymentId = payload.Object.Metadata["paymentId"];
            await paymentCreator.ApprovePayment(paymentId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.BadRequest();
        }
    }

    return Results.Ok();
});

app.MapPost("/api/text/generate", (IService service, IAiHandler aiHandler, [FromBody] TextRequest request) =>
{
    return service.GetText(request.Text, aiHandler);
})
.Accepts<TextRequest>("application/json");

app.MapGet("/api/health", () => 
{
    return "true";
});


    
//
// app.MapGet("/api/presentation/pptx/{id}", [Authorize] async (int id, IService service, IFileRepo fileRepo) =>
// {
//     try
//     {
//         var result = await service.GetPresenationPptx(id, fileRepo);
//         Console.WriteLine(result.Length+ "fd");
//         return Results.File(
//             result, 
//             "application/pptx", 
//             $"presentation_{DateTime.Now:yyyyMMddHHmmss}.pptx");
//     }
//     catch (Exception ex)
//     {
//         return Results.BadRequest($"Ошибка при создании презентации: {ex.Message}");
//     }
// });



app.Run();


string FindSolutionRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory != null)
    {
        if (directory.GetFiles("*.sln").Any())
            return directory.FullName;
        directory = directory.Parent;
    }
    return Directory.GetCurrentDirectory();
}

