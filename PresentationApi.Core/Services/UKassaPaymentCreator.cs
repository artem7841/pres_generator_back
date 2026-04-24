using System.Globalization;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Newtonsoft.Json;
using PresentationApi.ModelsBD;
using PresentationCreator.interfaces;

namespace PresentationCreator;

public class UKassaPaymentCreator : IPaymentCreator
{
    private HttpClient _client;
    private string _shopId;
    private string _secretKey;
    private IPaymentRepo _paymentRepo;
    private IUserRepo _userRepo;
    
    public UKassaPaymentCreator(HttpClient client, IPaymentRepo paymentRepo, IUserRepo userRepo)
    {
        _client = client;
        _shopId = Environment.GetEnvironmentVariable("YOO_SHOP_ID");;
        _secretKey = Environment.GetEnvironmentVariable("YOO_SECERET_KEY");
        _paymentRepo = paymentRepo;
        _userRepo = userRepo;
    }

    public async Task<string> CreatePayment(string price, string return_url, int userId)
    {

        var authToken = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_shopId}:{_secretKey}")
        );

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", authToken);
        
        var payment = await _paymentRepo.Create(new Payment()
        {
            UserId = userId,
            Amount = price,
            Status = "Pending",
            CreatedAt = DateTime.Now,
        });


        var paymentData = new
        {
            amount = new { value = price, currency = "RUB" },
            confirmation = new
            {
                type = "redirect",
                return_url = return_url
            },
            capture = true,
            description = "Оплата заказа #123",
            metadata = new
            {
                paymentId = payment.Id.ToString(),
            }
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(paymentData),
            Encoding.UTF8,
            "application/json"
        );
        
        _client.DefaultRequestHeaders.Add("Idempotence-Key", Guid.NewGuid().ToString());

        var response = await _client.PostAsync(
            "https://api.yookassa.ru/v3/payments",
            content
        );

        var result = await response.Content.ReadAsStringAsync();
        dynamic json = JsonConvert.DeserializeObject(result);
        
        Console.WriteLine("json: " + json);
        Console.WriteLine("url: " + json.confirmation.confirmation_url);
        
        if (json.type == "error")
            throw new Exception(json);
        
        payment.YookassaPaymentId = json.id;

        await _paymentRepo.Update(payment);
            
        return json.confirmation.confirmation_url;
    }

    public async Task ApprovePayment(string id)
    {
        var payment = await _paymentRepo.GetPayment(id);

        if (payment == null)
        {
            Console.WriteLine($"Платеж с id {id} не найден");
            throw new Exception($"Payment {id} not found");
        }
        
        if (string.IsNullOrEmpty(payment.Amount))
        {
            Console.WriteLine($"Amount для платежа {id} пустой");
            throw new Exception($"Amount is empty for payment {id}");
        }
    
        payment.Status = "Paid";
        await _paymentRepo.Update(payment);
        
        if (!decimal.TryParse(payment.Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
        {
            Console.WriteLine($"Не удалось распарсить Amount: {payment.Amount}");
            throw new Exception($"Invalid amount format: {payment.Amount}");
        }

        DateTime newExpiryDate;
        switch (amount)
        {
            case 5m:
                newExpiryDate = DateTime.Now + TimeSpan.FromDays(30);
                Console.WriteLine($"Updating subscription for 30 days, expires: {newExpiryDate}");
                await _userRepo.UpdateSubscription(payment.UserId, payment.Id, newExpiryDate);
                break;
            case 499m:
                newExpiryDate = DateTime.Now + TimeSpan.FromDays(90);
                Console.WriteLine($"Updating subscription for 90 days, expires: {newExpiryDate}");
                await _userRepo.UpdateSubscription(payment.UserId, payment.Id, newExpiryDate);
                break;
            default:
                Console.WriteLine($"Unknown amount: {amount}, no subscription updated");
                break;
        }
        
        Console.WriteLine($"Payment {id} successfully approved");
    }
}