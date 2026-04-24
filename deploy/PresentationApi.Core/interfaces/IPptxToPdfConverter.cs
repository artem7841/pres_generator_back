namespace PresentationCreator.interfaces;

public interface IPptxToPdfConverter
{
    public Task<string> ConvertAsync(string inputFile, string outputDir);
}