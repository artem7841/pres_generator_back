using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;

namespace PresentationCreator.interfaces;

public interface ISlideController
{
    public Task BuildPresentationFromJson(
        string json,
        PresentationDocument doc,
        YandexImageSearchService service,
        IImageCache imageCache);

}