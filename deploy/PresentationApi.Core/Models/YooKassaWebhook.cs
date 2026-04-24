namespace PresentationCreator.Models;

public class YooKassaWebhook
{
    public string Event { get; set; }
    public YooKassaObject Object { get; set; }
}

public class YooKassaObject
{
    public string Id { get; set; }
    public string Status { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}