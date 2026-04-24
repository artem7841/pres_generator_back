using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationCreator.interfaces;
using PresentationCreator.Models;

namespace PresentationApi.Controllers;
 
[ApiController]
[Route("api/[controller]")]
public class TextController : ControllerBase
{
    private IService _service;
    private IAiHandler _aiHandler;

    public TextController(IService service, IAiHandler aiHandler)
    {
        _service = service;
        _aiHandler = aiHandler;
    }

    [HttpPost("generate")]
    [Consumes("application/json")]
    [Produces("text/plain")]
    public async Task<IResult> GenerateText([FromBody] TextRequest request)
    {
        var result = await _service.GetText(request.Text, _aiHandler);
        return Results.Text(result);
    }
}