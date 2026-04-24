using System.Text;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Configuration;
using PresentationApi.Core.Exeptions;
using PresentationCreator.interfaces;
using PresentationCreator.Models;

namespace PresentationCreator;

public class Service :  IService
{
    private Random _random =  new Random();
    private IAiHandler _aiHandler;
    private IPptxToPdfConverter _converter;
    private IFileRepo _fileRepo;
    private IUserRepo _userRepo;
    private ISlideController _slideController;

    public Service(IAiHandler aiHandler, IPptxToPdfConverter converter, IFileRepo fileRepo, 
        IUserRepo userRepo, ISlideController slideController)
    {
        _aiHandler = aiHandler;
        _converter = converter;
        _fileRepo = fileRepo;
        _userRepo = userRepo;
        _slideController = slideController;
    }

    public async Task<string> GetText(string prompt, IAiHandler aiHandler)
    {
        string promptText = "Нужно сгенерировать текст для доклада по промпту пользователя, только текст без лишней информации от ИИ. Вот запрос пользователя: ";
        var result = await aiHandler.SendMessageAsync(promptText + prompt, "gpt-5.4-nano", 3000);
        return  result.Choices[0].Message.Content;
    }

    public async Task<NewPresentation> GetPresenation(string prompt, string text,  int userId, string model)
    {
        
        var user = await _userRepo.GetUserById(userId);
        var dateNow =  DateTime.Now;
        
        if (!user.HasActiveSubscription)
        {
            var arr = await _fileRepo.GetAllFiles(userId);
            if (arr.Count() >= 3)
            {
                throw new GenerationsEndedException("Бесплатные генерации закончились!");
            }
        }
            
        if(user.SubscriptionExpiresAt < dateNow)
             throw new GenerationsEndedException("Подписка закончилась!");
        
        string pptxPath = Path.Combine(Directory.GetCurrentDirectory(), "pres.pptx");
        string newPptxPath = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString() +  ".pptx");
        string json = "";
        File.Copy(pptxPath, newPptxPath);
    
        using (PresentationDocument doc = PresentationDocument.Open(newPptxPath, true))
        {
            var promptAll = "Есть текст: " + text + File.ReadAllText("prompt.txt") + " так же нужно учесть пожелания пользователя: " + prompt;
            var response = await _aiHandler.SendMessageAsync(promptAll, model, 3000);
            
            var jsonPre = response.Choices[0].Message.Content;
            json = ExtractJson(jsonPre);
 
            await _slideController.BuildPresentationFromJson(json, doc);
            
            doc.Save();
        }
        
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string pdfPath = await _converter.ConvertAsync(newPptxPath, tempDir);
        byte[] pdfBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
        byte[] pptxBytes = await System.IO.File.ReadAllBytesAsync(newPptxPath);
        
        string resultId = await _fileRepo.AddFile(newPptxPath, json, text, prompt, userId, pdfBytes, pptxBytes);
        
        NewPresentation newPresentation = new NewPresentation();
        newPresentation.Id = resultId;
        newPresentation.Data = pdfBytes;
        
        try
        {
            File.Delete(newPptxPath);
            Directory.Delete(tempDir, true);
        }
        catch { Console.WriteLine("Файл pptx не удалось удалить!"); }
    
        return newPresentation;
    }
    
    public async Task<NewPresentation> CorrectPresenation(int presId, string newPrompt, int userId)
    {
        var user = await _userRepo.GetUserById(userId);
        var dateNow =  DateTime.Now;
        
        if (!user.HasActiveSubscription)
        {
            var arr = await _fileRepo.GetAllFiles(userId);
            if (arr.Count() >= 3)
            {
                throw new GenerationsEndedException("Бесплатные генерации закончились!"); 
            }
        }
            
        if(user.SubscriptionExpiresAt < dateNow)
            throw new GenerationsEndedException("Подписка закончилась!");
        
        string pptxPath = Path.Combine(Directory.GetCurrentDirectory(), "pres.pptx");
        string newPptxPath = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString() +  ".pptx");
        string json = "";
        var pres = await _fileRepo.GetFileById(presId);

        if (pres.UserId != userId || pres == null)
        {
            return null;
        }
        
        File.Copy(pptxPath, newPptxPath);
    
        using (PresentationDocument doc = PresentationDocument.Open(newPptxPath, true))
        {
            var prompt = "Есть текст: " + pres.Text + File.ReadAllText("prompt.txt");
            var promptAll = "Ты сгенерировал пезентацию для пользователя в виде json. промт был таклй " + prompt +  
                            " ты выдал такой json: " + pres.Json + " теперь пользователь написал правки исправь по ним: " + newPrompt; 
            var response = await _aiHandler.SendMessageAsync(promptAll, "gemini-3.1-flash-lite-preview", 3000);
            
            var jsonPre = response.Choices[0].Message.Content;
            json = ExtractJson(jsonPre);
 
            await _slideController.BuildPresentationFromJson(json, doc);
            
            doc.Save();
        }
        
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        string pdfPath = await _converter.ConvertAsync(newPptxPath, tempDir);
        byte[] pdfBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);
        byte[] pptxBytes = await System.IO.File.ReadAllBytesAsync(newPptxPath);
        
        string resultId = await _fileRepo.ChangeFile(presId, json, pres.Text, pres.Text + newPrompt, pdfBytes, pptxBytes);
        
        NewPresentation newPresentation = new NewPresentation();
        newPresentation.Id = resultId;
        newPresentation.Data = pdfBytes;
        
        try
        {
            File.Delete(newPptxPath);
            Directory.Delete(tempDir, true);
        }
        catch { Console.WriteLine("Файл pptx не удалось удалить!"); }
    
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