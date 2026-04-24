using Microsoft.EntityFrameworkCore;
using PresentationApi.Data;
using PresentationApi.ModelsBD;
using PresentationCreator.interfaces;

namespace PresentationApi.Infrastructure.repositories;

public class FileRepo : IFileRepo
{
    private AppDbContext _context;
    private DbSet<PresProject> _dbSet;

    public FileRepo(AppDbContext context)
    {
        _context = context;
        _dbSet = _dbSet = context.Set<PresProject>();;
    }

    public async Task<string> AddFile(string fullPath, string json, string text, 
        string presPrompt, int userId,  byte[] pdfBytes, byte[] pptxBytes)
    {
        var res = await _dbSet.AddAsync(new PresProject()
        {
            CreatedAt = DateTime.Now,
            FileName = fullPath,
            Json = json,
            Text = text,
            PresPrompt = presPrompt,
            UserId = userId,
            PdfBytes = pdfBytes,
            PptxBytes = pptxBytes
        });
        await _context.SaveChangesAsync();
        return res.Entity.Id.ToString();
    }

    public async Task<byte[]> GetFilepptx(int id)
    {
        var res = await _dbSet.Where(f => f.Id == id).FirstAsync();
        return res.PptxBytes;
    }

    public async Task<string> ChangeFile(int id, string json, string text, string presPrompt,  byte[] pdfBytes,
        byte[] pptxBytes)
    {
        var res = await _dbSet.Where(f => f.Id == id).FirstOrDefaultAsync();
    
        if (res == null)
            throw new ArgumentException($"File with id {id} not found");
        
        res.Json = json;
        res.Text = text;
        res.PresPrompt = presPrompt;
        res.PdfBytes = pdfBytes;
        res.PptxBytes = pptxBytes;
        
        await _context.SaveChangesAsync();
    
        return res.Id.ToString();
    }

    public async Task<PresProject> GetFileById(int id)
    {
        var res = await _dbSet.Where(f => f.Id == id).FirstAsync();
        return res;
    }

    public async Task<List<PresProject>> GetAllFiles(int id)
    {
        return await _dbSet.Where(f => f.UserId == id)
            .Select(f => new PresProject()
            {
                Id = f.Id,
                CreatedAt = f.CreatedAt,
                FileName = f.FileName,
                Json = f.Json,
                Text = f.Text,
                PresPrompt = f.PresPrompt,
                UserId = f.UserId,
                PdfBytes = f.PdfBytes,
            })
            .ToListAsync();
    }
}