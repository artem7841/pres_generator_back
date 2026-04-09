namespace PresentationCreator.Models;

public class Choice
{
    public int Index { get; set; }
    public MessageResponse Message { get; set; }
    public string FinishReason { get; set; }
}