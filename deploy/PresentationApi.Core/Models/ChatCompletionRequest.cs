namespace PresentationCreator.Models;

public class ChatCompletionRequest
{
    public string Model { get; set; }
    public int MaxTokens { get; set; }
    public Message[] Messages { get; set; }
}