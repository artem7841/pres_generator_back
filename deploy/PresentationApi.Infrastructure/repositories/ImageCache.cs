using Microsoft.Extensions.Caching.Distributed;
using PresentationCreator.interfaces;

namespace PresentationApi.Infrastructure.repositories;

public class ImageCache : IImageCache
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _expiry = TimeSpan.FromDays(3);
    
    public ImageCache(IDistributedCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }
    
    public async Task SetImage(string prompt, string url)
    {
        await _cache.SetStringAsync(prompt, url);
    }

    public async Task<string> GetImage(string prompt)
    {
        var res = await _cache.GetStringAsync(prompt);
        return res; 
    }

}