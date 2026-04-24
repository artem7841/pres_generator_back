using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationCreator.interfaces;
using PresentationCreator.Models;

namespace PresentationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayController : ControllerBase
{
    private readonly IPaymentCreator _paymentCreator;

    public PayController(IPaymentCreator paymentCreator)
    {
        _paymentCreator = paymentCreator;
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IResult> Pay(
        [FromBody] OrderRequest request, 
        [FromServices] ICurrentUserService currentUserService)
    {
        Console.WriteLine(request.Amount);
        var userId = currentUserService.GetUserId();

        var url = await _paymentCreator.CreatePayment(request.Amount, "https://prezaai.ru", userId);
        return Results.Ok(url);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IResult> Webhook([FromBody] YooKassaWebhook payload)
    {
        Console.WriteLine("Webhook " + payload);

        if (payload.Event == "payment.succeeded")
        {
            try
            {
                var paymentId = payload.Object.Metadata["paymentId"];
                await _paymentCreator.ApprovePayment(paymentId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Results.BadRequest();
            }
        }

        return Results.Ok();
    }
}