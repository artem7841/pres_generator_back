namespace PresentationCreator.interfaces;

public interface IEmailService
{
    public Task<bool> SendCodeAsync(string email, string code);
}