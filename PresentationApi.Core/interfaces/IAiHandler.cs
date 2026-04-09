using PresentationCreator.Models;

namespace PresentationCreator.interfaces;

public interface IAiHandler
{
    public Task<ChatCompletionResponse> SendMessageAsync(
        string message,
        string model,
        int maxTokens);
}