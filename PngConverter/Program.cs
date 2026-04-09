using System;
using System.Linq;


class Program
{
    static async Task Main()
    {
        var converter = new PptxToPngConverter(
            @"C:\Program Files\LibreOffice\program\soffice.exe"
        );

        var images = await converter.ConvertAsync(
            "pres.pptx",
            "output"
        );

        foreach (var img in images)
        {
            Console.WriteLine(img);
        }
    }
    
}