using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Http;
using PresentationCreator.interfaces; // Добавьте этот using

public class PdfToPptxDockerConverter : IPptxToPdfConverter
{
    private readonly HttpClient _httpClient;

    public PdfToPptxDockerConverter(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("http://converter:3000/");
    }

    public async Task<string> ConvertAsync(string inputFile, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(inputFile));
        
        form.Add(fileContent, "files", Path.GetFileName(inputFile));

        var response = await _httpClient.PostAsync("forms/libreoffice/convert", form);
        response.EnsureSuccessStatusCode();

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        string pdfPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(inputFile) + ".pdf");
        
        await File.WriteAllBytesAsync(pdfPath, pdfBytes);
        return pdfPath;
    }
}