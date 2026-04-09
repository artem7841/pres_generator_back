using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class PptxToPngConverter
{
    private readonly string _sofficePath;

    public PptxToPngConverter(string sofficePath)
    {
        _sofficePath = sofficePath;
    }

    public async Task<string> ConvertAsync(string inputFile, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        var pdfPath = Path.Combine(outputDir, "temp.pdf");

        // 1. PPTX → PDF
        await RunProcess($"--headless --convert-to pdf --outdir \"{outputDir}\" \"{inputFile}\"");

        // имя pdf = имя pptx
        var generatedPdf = Path.Combine(
            outputDir,
            Path.GetFileNameWithoutExtension(inputFile) + ".pdf"
        );
        
        return Directory.GetFiles(outputDir, "pres.pdf").FirstOrDefault();
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
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception(await process.StandardError.ReadToEndAsync());
    }
}