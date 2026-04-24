using System.Text;
using System.Text.Json;
using PresentationCreator.interfaces;
using PresentationCreator.Models;

namespace PresentationCreator;

public class AiHandler : IAiHandler
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = Environment.GetEnvironmentVariable("API_KEY_AI");
    private const string BaseUrl = "https://api.aitunnel.ru/v1/chat/completions";

    public AiHandler()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }
    
    public async Task<ChatCompletionResponse> SendMessageAsync(
        string message, 
        string model = "claude-haiku-4.5", 
        int maxTokens = 5000)
    {
        var request = new ChatCompletionRequest
        {
            Model = model,
            MaxTokens = maxTokens,
            Messages = new[]
            {
                new Message
                {
                    Role = "user",
                    Content = message
                }
            }
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        try
        {
            var response = await _httpClient.PostAsync(BaseUrl, content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"API request failed: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

