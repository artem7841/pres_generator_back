namespace PresentationCreator.Models;

public class SlideModel
{
    public string backgroundColor { get; set; }
    public string backgroundColor2 { get; set; }
    public List<TextModel> texts { get; set; }
    public List<ImageModel> images { get; set; }
    public TableModel table { get; set; }
}