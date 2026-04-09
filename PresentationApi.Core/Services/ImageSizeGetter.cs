namespace PresentationCreator;

public class ImageSizeGetter
{
    public static async Task<long> GetImageHeightAsync(string imageUrl, long width)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(10);
            
                using (var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var image = System.Drawing.Image.FromStream(stream);
                        var coef = (double)image.Width / width; 
                        var result = (long)(image.Height / coef);
                        return result; 
                    }
                }
            }
        }
        catch
        {
            return  (long)(width / 1.7); 
        }
    }
}