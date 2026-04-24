using System.Net;
using System.Xml;
using System.Xml.Linq;
using PresentationCreator.Models;

namespace PresentationCreator;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class YandexImageSearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = Environment.GetEnvironmentVariable("YANDEX_API_KEY");
    private readonly string _folderId = Environment.GetEnvironmentVariable("YANDEX_FOLDER_ID");

    public YandexImageSearchService()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            if (errors != System.Net.Security.SslPolicyErrors.None)
            {
                Console.WriteLine($"SSL ошибка: {errors}");
            }
            return true;
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Api-Key {_apiKey}");
    }

    public async Task<List<ImageResult>> SearchImagesAsync(string query, int count = 5)
    {
        var requestBody = CreateRequestBody(query, count);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(
                "https://searchapi.api.cloud.yandex.net/v2/image/search", content);

            Console.WriteLine($"Статус ответа: {(int)response.StatusCode} {response.ReasonPhrase}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Ошибка API: {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                                  $"Ответ: {errorContent}");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            
            var apiResponse = JsonConvert.DeserializeObject<YandexImageSearchResponse>(responseContent);
            var decodedXml = DecodeBase64ToXml(apiResponse.RawData);
            return ParseImagesFromXml(decodedXml, count);
        }
        catch (HttpRequestException httpEx)
        {
            throw new Exception($"HTTP ошибка: {httpEx.Message}\n" +
                             $"Inner: {httpEx.InnerException?.Message}", httpEx);
        }
        catch (Exception ex)
        {
            throw new Exception($"Общая ошибка при поиске изображений: {ex.Message}", ex);
        }
    }

    private string CreateRequestBody(string query, int count)
    {
        var body = new
        {
            folderId = _folderId,
            query = new
            {
                searchType = "SEARCH_TYPE_RU",
                queryText = query,
                page = 0,
                docsOnPage = count
            },
            imageSpec = new
            {
                format = "IMAGE_FORMAT_JPEG",
                size = "IMAGE_SIZE_MEDIUM",
                orentation = "IMAGE_ORIENTATION_HORIZONTAL"
            },
        };
        return JsonConvert.SerializeObject(body, Formatting.None);
    }

    private string DecodeBase64ToXml(string base64String)
    {
        var bytes = Convert.FromBase64String(base64String);
        return Encoding.UTF8.GetString(bytes);
    }

    private List<ImageResult> ParseImagesFromXml(string xml, int count)
{
    var images = new List<ImageResult>();

    try
    {
        var doc = XDocument.Parse(xml);
        XNamespace ns = doc.Root?.GetDefaultNamespace() ?? "";

        var docElements = doc.Descendants("doc").Take(count);

        foreach (var element in docElements)
        {
            try
            {
                var image = new ImageResult();
                
                var imageProps = element.Element(ns + "image-properties");

                if (imageProps != null)
                {
                    var imageLink = imageProps.Element(ns + "image-link");
                    if (imageLink != null)
                    {
                        image.Url = imageLink.Value.Trim();
                    }
                    
                    var width = imageProps.Element(ns + "original-width");
                    if (width != null && int.TryParse(width.Value, out int w))
                        image.Width = w;

                    var height = imageProps.Element(ns + "original-height");
                    if (height != null && int.TryParse(height.Value, out int h))
                        image.Height = h;
                }
                
                var titleElement = element.Element(ns + "title");
                if (titleElement != null)
                {
                    image.Title = titleElement.Value.Trim();
                }
                
                if (string.IsNullOrEmpty(image.Url))
                {
                    var fallbackUrl = element.Element(ns + "url");
                    if (fallbackUrl != null)
                        image.Url = fallbackUrl.Value.Trim();
                }

                if (!string.IsNullOrEmpty(image.Url))
                {
                    images.Add(image);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка элемента: {ex.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        throw new Exception($"Ошибка парсинга XML: {ex.Message}", ex);
    }

    return images;
}

}
