using PresentationApi.ModelsBD;

namespace PresentationCreator.interfaces;

public interface ICodeRepo
{
    Task<bool> AddCode(string code, string email);
    Task<LoginCode> GetLastCode(string email);
}