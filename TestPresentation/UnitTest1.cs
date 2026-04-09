using DocumentFormat.OpenXml.Packaging;
using DotNetEnv;
using PresentationCreator;

namespace TestPresentation;

public class Tests
{
    private AiHandler _handler;
    private YandexImageSearchService _yandexImageSearchService;
    private SlideController _slideController;
    [SetUp]
    public void Setup()
    {
        var solutionRoot = FindSolutionRoot();
        var envPath = Path.Combine(solutionRoot, ".env");
        
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
        }
        else
        {
            Console.WriteLine($"Warning: .env not found at {envPath}");
            Env.Load();
        }
        
        _handler = new AiHandler();
        _yandexImageSearchService = new YandexImageSearchService();
        _slideController = new SlideController();
    }

    [Test]
    public async Task AiHandlerTest()
    {
        var result = await _handler.SendMessageAsync("просто напиши слово Привет");
        var expected = "Привет";
        
        Assert.That(result.Choices[0].Message.Content, Is.EqualTo(expected));
    }
    
    [Test]
    public async Task YandexImageServiceTest()
    {
        var result = await _yandexImageSearchService.SearchImagesAsync("bmw", 3);
        
        Assert.That(result, Has.Count.EqualTo(3));
    }
    
    [Test]
    public async Task SlideControllerTest()
    {
        string json =
            "{\n  \"slides\": [\n    {\n      \"backgroundColor\": \"2E86AB\",\n      \"backgroundColor2\": \"1B4F72\",\n      \"texts\": [\n        {\n          \"value\": \"Растительный мир Урала: Голосеменные растения\",\n          \"fontSize\": 50,\n          \"fontFamily\": \"Calibri\",\n          \"fontColor\": \"FFFFFF\",\n          \"X\": 50,\n          \"Y\": 50,\n          \"width\": 1180,\n          \"height\": 120\n        },\n        {\n          \"value\": \"Удивительное путешествие в мир хвойных деревьев и кустарников нашего края.\",\n          \"fontSize\": 30,\n          \"fontFamily\": \"Calibri\",\n          \"fontColor\": \"E0E0E0\",\n          \"X\": 50,\n          \"Y\": 200,\n          \"width\": 600,\n          \"height\": 100\n        }\n      ],\n      \"images\": [\n        {\n          \"prompt\": \"Beautiful Ural mountains landscape with pine forest\",\n          \"X\": 700,\n          \"Y\": 200,\n          \"width\": 500\n        }\n      ]\n    }]}";
        
        
        Assert.DoesNotThrow(() =>
        {
            using (PresentationDocument doc = PresentationDocument.Open("pres.pptx", true))
            {
                _slideController.BuildPresentationFromJson(json, doc, _yandexImageSearchService).Wait();
            }
        });
    }
    

    
    private string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null)
        {
            if (directory.GetFiles("*.sln").Any())
                return directory.FullName;
            directory = directory.Parent;
        }
        return Directory.GetCurrentDirectory();
    }
}