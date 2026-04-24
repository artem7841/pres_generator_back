namespace PresentationCreator.interfaces;

public interface ICurrentUserService
{
    public int GetUserId();
    public string GetUserEmail();
    public bool IsAuthenticated();
}