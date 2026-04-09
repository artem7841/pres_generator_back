using PresentationApi.ModelsBD;

namespace PresentationCreator.interfaces;

public interface IFileRepo
{
    Task<string> AddFile(string fullPath, string json, string text, string presPrompt, int userId, byte[] pdfBytes, byte[] pptxBytes);
    Task<string> ChangeFile(int id, string json, string text, string presPrompt,  byte[] pdfBytes, byte[] pptxBytes);
    Task<List<PresProject>> GetAllFiles(int id);
    Task<PresProject> GetFileById(int id);
    Task<byte[]> GetFilepptx(int id);
}