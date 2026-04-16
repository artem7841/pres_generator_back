namespace PresentationCreator.interfaces;

public interface IPaymentCreator
{
    public Task<string> CreatePayment(string price,  string url, int userId);
    public Task ApprovePayment(string id);
}