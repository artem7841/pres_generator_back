namespace PresentationApi.Core.Exeptions;

public class GenerationsEndedException : Exception
{
    public GenerationsEndedException(string message) : base(message) { }
}