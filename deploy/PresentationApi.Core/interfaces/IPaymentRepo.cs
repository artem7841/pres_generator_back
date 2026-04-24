using PresentationApi.ModelsBD;

namespace PresentationCreator.interfaces;

public interface IPaymentRepo
{
    Task<Payment> Create(Payment payment);
    Task Update(Payment payment);
    Task<List<Payment>> GetAllPayments(string idUser);
    Task<Payment?> GetPayment(string paymentId);
}