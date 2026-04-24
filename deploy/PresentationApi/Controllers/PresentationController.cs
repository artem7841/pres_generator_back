using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresentationCreator.interfaces;
using PresentationCreator.Models;

namespace PresentationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PresentationController : ControllerBase
{
    private IService _service;
    private IFileRepo _fileRepo;
    private ICurrentUserService _currentUserService;

    public PresentationController( IService service, IFileRepo fileRepo, ICurrentUserService currentUserService)
    {
        _service = service;
        _fileRepo = fileRepo;
        _currentUserService = currentUserService;
    }

    [HttpPost("generate")]
    [Authorize]
    public async Task<IResult> Generate(
        [FromBody] PresentationRequest request)
    {
        var userId = _currentUserService.GetUserId();
    
        var result = await _service.GetPresenation(
            request.Prompt, 
            request.Text, 
            userId, 
            "gemini-3.1-flash-lite-preview"
        );
    
        HttpContext.Response.Headers.Append("X-Presentation-Id", result.Id);
    
        return Results.File(
            result.Data, 
            "application/pdf", 
            $"presentation_{DateTime.Now:yyyyMMddHHmmss}.pdf");
    }
    
    [HttpPost("correct")]
    [Authorize]
    public async Task<IResult> Correct([FromBody] PresentationCorrectRequest request,
        [FromServices] ICurrentUserService currentUserService)
    {
        var userId = currentUserService.GetUserId();
    
        var result = await _service.CorrectPresenation(
            request.id, 
            request.prompt, 
            userId);

        if (result ==  null)
            return Results.NotFound();

        return Results.File(
            result.Data,
            "application/pdf",
            $"presentation_{DateTime.Now:yyyyMMddHHmmss}.pdf");
    }
    
    [HttpGet("all")]
    [Authorize]
    public async Task<IResult> GetAllPres([FromServices] ICurrentUserService currentUserService)
    {
        var userId = currentUserService.GetUserId();
        var files = await _fileRepo.GetAllFiles(userId);
        return Results.Ok(files);
    }
    
}