namespace PresentationCreator.Models;

public class ChatCompletionResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public long Created { get; set; }
    public string Model { get; set; }
    public Choice[] Choices { get; set; }
    public Usage Usage { get; set; }
}