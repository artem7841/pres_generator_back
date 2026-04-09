using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using PresentationCreator.interfaces;

public class PptxToPdfConverter : IPptxToPdfConverter
{
    private readonly string _sofficePath = @"C:\\Program Files\\LibreOffice\\program\\soffice.exe";
    
    public async Task<string> ConvertAsync(string inputFile, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        // 1. PPTX → PDF
        await RunProcess($"--headless --convert-to pdf --outdir \"{outputDir}\" \"{inputFile}\"");

        // 2. Получаем путь к сгенерированному PDF
        string pdfPath = Path.Combine(
            outputDir,
            Path.GetFileNameWithoutExtension(inputFile) + ".pdf"
        );
        
        // Проверяем, существует ли файл
        if (!File.Exists(pdfPath))
        {
            throw new FileNotFoundException($"PDF файл не был создан по пути: {pdfPath}");
        }
        
        return pdfPath;
    }

    private async Task RunProcess(string args)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _sofficePath,
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            string error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"LibreOffice error (exit code {process.ExitCode}): {error}");
        }
    }
}