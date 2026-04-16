namespace PresentationCreator.Models;

public class TableModel
{
    public long X { get; set; }
    public long Y { get; set; }
    public long width { get; set; }
    public long height { get; set; }
    public List<List<TextTableModel>> data { get; set; }
}