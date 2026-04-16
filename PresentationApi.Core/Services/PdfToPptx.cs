using System.Diagnostics;
using System.Runtime.InteropServices;
using PresentationCreator.interfaces;

namespace PresentationCreator;

public class PdfToPptx : IPptxToPdfConverter
{
    private readonly string _sofficePath;
    
    public PdfToPptx()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _sofficePath = @"C:\Program Files\LibreOffice\program\soffice.exe";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _sofficePath = "/usr/bin/soffice";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _sofficePath = "/Applications/LibreOffice.app/Contents/MacOS/soffice";
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported operating system");
        }
    }
    
    public async Task<string> ConvertAsync(string inputFile, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
   
        var args = $"--headless --convert-to pdf --outdir \"{outputDir}\" \"{inputFile}\"";
        
        await RunProcess(args);
        
        string pdfPath = Path.Combine(
            outputDir,
            Path.GetFileNameWithoutExtension(inputFile) + ".pdf"
        );
        
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
                CreateNoWindow = true,
                // Важно для Linux контейнера
                EnvironmentVariables = {
                    ["HOME"] = "/tmp",
                    ["USER"] = "root"
                }
            }
        };

        try
        {
            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"LibreOffice error (exit code {process.ExitCode}): {error}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to start LibreOffice at {_sofficePath}. Error: {ex.Message}");
        }
    }
    
}