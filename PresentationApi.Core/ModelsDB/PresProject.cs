namespace PresentationApi.ModelsBD;

public class PresProject
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FileName { get; set; }
    public string Json { get; set; }
    public string Text { get; set; }
    public string PresPrompt { get; set; }
    public DateTime CreatedAt { get; set; }
    public byte[] PdfBytes { get; set; }
    public byte[] PptxBytes { get; set; }
    public User? User { get; set; }
}
