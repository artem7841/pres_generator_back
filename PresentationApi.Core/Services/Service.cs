using System.Text;
using DocumentFormat.OpenXml.Packaging;
using PresentationCreator.interfaces;
using PresentationCreator.Models;

namespace PresentationCreator;

public class Service :  IService
{
    private Random _random =  new Random();
    public async Task<string> GetText(string prompt, IAiHandler aiHandler)
    {
        string promptText = "Нужно сгенерировать текст для доклада по промпту пользователя, только текст без лишней информации от ИИ. Вот запрос пользователя: ";
        var result = await aiHandler.SendMessageAsync(promptText + prompt, "gpt-5.4-nano", 3000);
        return  result.Choices[0].Message.Content;
    }

    public async Task<NewPresentation> GetPresenation(string prompt, string text,  int userId, string model,
        YandexImageSearchService yandexImageSearchService, ISlideController controller,
        IAiHandler aiHandler, IPptxToPdfConverter converter, IFileRepo fileRepo)
    {
        string pptxPath = Path.Combine(Directory.GetCurrentDirectory(), "pres.pptx");
        string newPptxPath = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString() +  ".pptx");
        string json = "";
        File.Copy(pptxPath, newPptxPath);
    
        using (PresentationDocument doc = PresentationDocument.Open(newPptxPath, true))
        {
            var promptAll = "Есть текст: " + text + File.ReadAllText("prompt.txt") + " так же нужно учесть пожелания пользователя: " + prompt;
            var response = await aiHandler.SendMessageAsync(promptAll, model, 3000);
            
            var jsonPre = response.Choices[0].Message.Content;
            json = ExtractJson(jsonPre);
 
            await controller.BuildPresentationFromJson(json, doc, yandexImageSearchService);
            
            doc.Save();
        }
        
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string pdfPath = await converter.ConvertAsync(newPptxPath, tempDir);
        byte[] pdfBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
        byte[] pptxBytes = await System.IO.File.ReadAllBytesAsync(newPptxPath);
        
        string resultId = await fileRepo.AddFile(newPptxPath, json, text, prompt, userId, pdfBytes, pptxBytes);
        
        NewPresentation newPresentation = new NewPresentation();
        newPresentation.Id = resultId;
        newPresentation.Data = pdfBytes;
        
        try
        {
            File.Delete(newPptxPath);
            Directory.Delete(tempDir, true);
        }
        catch { /* Логируем ошибку, но не блокируем основной процесс */ }
    
        return newPresentation;
    }
    
    public async Task<NewPresentation> CorrectPresenation(int presId, string newPrompt, int userId, YandexImageSearchService yandexImageSearchService, ISlideController controller,
        IAiHandler aiHandler, IPptxToPdfConverter converter, IFileRepo fileRepo)
    {
        string pptxPath = Path.Combine(Directory.GetCurrentDirectory(), "pres.pptx");
        string newPptxPath = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString() +  ".pptx");
        string json = "";
        var pres = await fileRepo.GetFileById(presId);

        if (pres.UserId != userId || pres == null)
        {
            return null;
        }
        
        File.Copy(pptxPath, newPptxPath);
    
        using (PresentationDocument doc = PresentationDocument.Open(newPptxPath, true))
        {
            var promptAll = "Ты сгенерировал пезентацию для пользователя в в иде json. Текст для неее был такой: " +
                            pres.Text + " ты выдал такой json: " + pres.Json + " теперь пользователь написал правки исправь по ним: " + newPrompt; 
            var response = await aiHandler.SendMessageAsync(promptAll, "gemini-3.1-flash-lite-preview", 3000);
            
            var jsonPre = response.Choices[0].Message.Content;
            json = ExtractJson(jsonPre);
 
            await controller.BuildPresentationFromJson(json, doc, yandexImageSearchService);
            
            doc.Save();
        }
        
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string pdfPath = await converter.ConvertAsync(newPptxPath, tempDir);
        byte[] pdfBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
        byte[] pptxBytes = await System.IO.File.ReadAllBytesAsync(newPptxPath);
        
        string resultId = await fileRepo.ChangeFile(presId, json, pres.Text, pres.Text + newPrompt, pdfBytes, pptxBytes);
        
        NewPresentation newPresentation = new NewPresentation();
        newPresentation.Id = resultId;
        newPresentation.Data = pdfBytes;
        
        try
        {
            File.Delete(newPptxPath);
            Directory.Delete(tempDir, true);
        }
        catch { /* Логируем ошибку, но не блокируем основной процесс */ }
    
        return newPresentation;
    }

    public async Task<byte[]> GetPresenationPptx(int id, IFileRepo fileRepo)
    {
        return await fileRepo.GetFilepptx(id);
    }
    
    private static string ExtractJson(string response)
    {
        int startIndex = response.IndexOf('{');
        if (startIndex == -1)
        {
            throw new InvalidOperationException("JSON не найден в ответе AI");
        }
        
        int endIndex = response.LastIndexOf('}');
        if (endIndex == -1 || endIndex < startIndex)
        {
            throw new InvalidOperationException("Не найдена закрывающая скобка JSON");
        }

        string json = response.Substring(startIndex, endIndex - startIndex + 1);
    
        return json;
    }

    
}