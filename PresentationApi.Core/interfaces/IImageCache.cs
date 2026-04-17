using PresentationCreator.Models;

namespace PresentationCreator.interfaces;

public interface IImageCache
{
    public Task SetImage(string prompt, string url);
    public Task<string> GetImage(string prompt);
}