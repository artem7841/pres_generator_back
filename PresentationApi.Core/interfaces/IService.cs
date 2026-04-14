using PresentationCreator.Models;

namespace PresentationCreator.interfaces;

public interface IService
{
    public Task<string> GetText(string prompt, IAiHandler aiHandler);
    public Task<NewPresentation> GetPresenation(string prompt, string text, int userId, string model, YandexImageSearchService yandexImageSearchService, 
        ISlideController controller, IAiHandler aiHandler, IPptxToPdfConverter pptxToPdfConverter, IFileRepo fileRepo);

    public Task<byte[]> GetPresenationPptx(int id, IFileRepo fileRepo);
    public Task<NewPresentation> CorrectPresenation(int presId, string newPrompt, int userId, YandexImageSearchService yandexImageSearchService, ISlideController controller,
        IAiHandler aiHandler, IPptxToPdfConverter converter, IFileRepo fileRepo);
}