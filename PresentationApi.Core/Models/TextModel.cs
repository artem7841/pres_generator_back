namespace PresentationCreator.Models;

public class TextModel
{
    public string value { get; set; }
    public int fontSize { get; set; }
    public string fontFamily { get; set; }
    public string fontColor { get; set; }
    public long X { get; set; }
    public long Y { get; set; }
    public long width { get; set; }
    public long height { get; set; }
}

public class TextTableModel
{
    public string value { get; set; }
    public int fontSize { get; set; }
    public string fontFamily { get; set; }
    public string fontColor { get; set; }
    public string center { get; set; }
    public string  backgroundColor { get; set; }
}