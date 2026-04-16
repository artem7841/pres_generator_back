using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using PresentationCreator.interfaces;
using PresentationCreator.Models;
namespace PresentationCreator;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;

public class SlideController : ISlideController
{
    const long emuPerPixel = 9525; 

    public async Task BuildPresentationFromJson(
        string json,
        PresentationDocument doc,
        YandexImageSearchService service,
        IImageCache imageCache)
    {
        var model = JsonSerializer.Deserialize<PresentationModel>(json);
        
        Console.WriteLine($"Всего слайдов: {model.slides.Count}");
        Console.WriteLine(json);
        
        int slideIndex = 0;
        foreach (var slideData in model.slides)
        {
            slideIndex++;
            Console.WriteLine($"\n=== Обработка слайда {slideIndex} ===");
            
            try
            {
                // 1. создаём слайд
                var slidePart = AddSlide(
                    doc,
                    slideData.backgroundColor,
                    slideData.backgroundColor2
                );
                
                Console.WriteLine($"Слайд {slideIndex} создан");
                
                // 2. добавляем тексты
                if (slideData.texts != null)
                {
                    Console.WriteLine($"Добавление {slideData.texts.Count} текстов");
                    foreach (var text in slideData.texts)
                    {
                        AddText(
                            slidePart,
                            text.value,
                            text.fontSize,
                            text.fontColor ?? "000000",
                            text.fontFamily ?? "Calibri",
                            text.X,
                            text.Y,
                            text.width,
                            text.height
                        );
                    }
                }
                
                // 3. добавляем картинки
                if (slideData.images != null)
                {
                    Console.WriteLine($"Добавление {slideData.images.Count} изображений");
                    int imgIndex = 0;
                    foreach (var image in slideData.images)
                    {
                        imgIndex++;
                        try
                        {
                            Console.WriteLine($"\n--- Изображение {imgIndex} ---");
                            var imageFromCache = await imageCache.GetImage(image.prompt);
                            using var cts = new CancellationTokenSource();
                            
                            if (imageFromCache != null)
                            {
                                await AddImageFromUrlAsync(slidePart, imageFromCache,
                                    image.X, image.Y, image.width, cts.Token);
                            }
                            else
                            {
                                var imagePathsTask = service.SearchImagesAsync(image.prompt);
                                imagePathsTask.Wait();
                                var imagePaths = await imagePathsTask;
                                var tasks = new List<Task<string>>();
                                
                                foreach (var imagePath in imagePaths)
                                {
                                    if (!string.IsNullOrEmpty(imagePath.Url))
                                    {
                                        var task = AddImageFromUrlAsync(slidePart, imagePath.Url,
                                            image.X, image.Y, image.width, cts.Token);
                                        tasks.Add(task);
                                    }
                                }

                                var url = await GetFirstSuccessfulTaskWithCancellationAsync(tasks, cts);
                                await imageCache.SetImage(image.prompt, url);
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при добавлении изображения {imgIndex}: {ex.Message}");
                            Console.WriteLine($"Stack trace: {ex.StackTrace}");
                        }
                    }
                }

                if (slideData.table != null)
                {
                    try
                    {
                        var table = slideData.table.data;
                        AddTable(slidePart, table.Count, table[0].Count,
                            slideData.table.X, slideData.table.Y, slideData.table.width, slideData.table.height, table);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    
                }
                
                slidePart.Slide.Save();
                Console.WriteLine($"Слайд {slideIndex} сохранен");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании слайда {slideIndex}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        Console.WriteLine("\n=== Создание презентации завершено ===");
    }
    
    
    private async Task<string> GetFirstSuccessfulTaskWithCancellationAsync(
        List<Task<string>> tasks,
        CancellationTokenSource cts)
    {
        var remainingTasks = new List<Task<string>>(tasks);
        var exceptions = new List<Exception>();
    
        while (remainingTasks.Any())
        {
            var completedTask = await Task.WhenAny(remainingTasks);
        
            try
            {
                string result = await completedTask;
                
                cts.Cancel();
                
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                remainingTasks.Remove(completedTask);
                exceptions.Add(ex);
            }
        }
    
        throw new AggregateException("Все попытки загрузки изображений не удались", exceptions);
    }
    
        public SlidePart AddSlide(PresentationDocument doc, string backgroundColor = null, string bacbackgroundColor2 = null)
        {
            var presentationPart = doc.PresentationPart;
            
            if (!presentationPart.SlideMasterParts.Any())
            {
                throw new InvalidOperationException("Нет SlideMasterPart в презентации");
            }

            var slideMasterPart = presentationPart.SlideMasterParts.First();

            if (!slideMasterPart.SlideLayoutParts.Any())
            {
                throw new InvalidOperationException("Нет SlideLayoutPart в SlideMaster");
            }

            var slideLayoutPart = slideMasterPart.SlideLayoutParts.First();

            // Создаём новый слайд
            SlidePart newSlidePart = presentationPart.AddNewPart<SlidePart>();
            newSlidePart.AddPart(slideLayoutPart);

            newSlidePart.Slide = new Slide(
                new CommonSlideData(new ShapeTree()),
                new ColorMapOverride(new A.MasterColorMapping())
            );

            // Копируем базовую структуру из layout (ВАЖНО)
            newSlidePart.Slide.CommonSlideData = 
                (CommonSlideData)slideLayoutPart.SlideLayout.CommonSlideData.CloneNode(true);

            // Ищем shape с текстом
            var shape = newSlidePart.Slide.Descendants<Shape>().ToArray();
            var head = shape[0];
            var par = shape[1];
            var par2 = shape[2];

            head.Remove();
            par.Remove();
            par2.Remove();
            
            RemoveUnwantedPlaceholders(newSlidePart);
            if (!string.IsNullOrEmpty(backgroundColor))
            {
                if(!string.IsNullOrEmpty(bacbackgroundColor2))
                    SetGradientBackground(newSlidePart, backgroundColor, bacbackgroundColor2);
                else
                {
                    SetSlideBackground(newSlidePart, backgroundColor);
                }
            }
            AddToSlideList(presentationPart, newSlidePart);

            return newSlidePart;
        }
        

        private void SetGradientBackground(SlidePart slidePart, string startColorHex, string endColorHex, int angle = 90)
        {
            var slide = slidePart.Slide;

            slide.CommonSlideData.Background?.Remove();

            var background = new Background();
            var backgroundProperties = new BackgroundProperties();

            var gradientFill = new A.GradientFill();

            var gradientStops = new A.GradientStopList();

            // 0%
            var stop1 = new A.GradientStop() { Position = 0 };
            stop1.Append(new A.RgbColorModelHex() { Val = startColorHex.Replace("#", "") });
            gradientStops.Append(stop1);

            // 100%
            var stop2 = new A.GradientStop() { Position = 100000 };
            stop2.Append(new A.RgbColorModelHex() { Val = endColorHex.Replace("#", "") });
            gradientStops.Append(stop2);

            gradientFill.Append(gradientStops);

            var linearGradient = new A.LinearGradientFill()
            {
                Angle = angle * 60000, 
                Scaled = true
            };

            gradientFill.Append(linearGradient);
            backgroundProperties.Append(gradientFill);
            background.Append(backgroundProperties);

            slide.CommonSlideData.Background = background;

            slide.Save(); 
        }
        
        private void SetSlideBackground(SlidePart slidePart, string colorHex)
        {
            var slide = slidePart.Slide;
            
            if (slide.CommonSlideData.Background != null)
            {
                slide.CommonSlideData.Background.Remove();
            }

            var background = new Background();
            var backgroundProperties = new BackgroundProperties();

            var solidFill = new A.SolidFill();
            var rgbColor = new A.RgbColorModelHex()
            {
                Val = colorHex.Replace("#", "")
            };
            solidFill.Append(rgbColor);
        
            backgroundProperties.Append(solidFill);
            background.Append(backgroundProperties);
 
            slide.CommonSlideData.Background = background;
        }

        public void AddText(SlidePart slidePart, string text, Int32Value fontSize, string fontColor = "000000", string fontFamaly = "Calibri",
            long x = 500, long y = 200, long width = 800, long height = 300)
        {
            fontSize *= 85;
            x *= emuPerPixel;
            y *= emuPerPixel;
            width *= emuPerPixel;
            height *= emuPerPixel;
            var shapeTree = slidePart.Slide.CommonSlideData.ShapeTree;

            if (shapeTree == null)
            {
                shapeTree = new ShapeTree();
                slidePart.Slide.CommonSlideData.ShapeTree = shapeTree;
            }

            uint nextId = GetNextShapeId(shapeTree);


            var textShape = new Shape(
                new NonVisualShapeProperties(
                    new NonVisualDrawingProperties() { Id = nextId, Name = $"Text_{nextId}" },
                    new NonVisualShapeDrawingProperties(),
                    new ApplicationNonVisualDrawingProperties()
                ),
                new ShapeProperties(
                    new A.Transform2D(
                        new A.Offset() { X = x, Y = y },
                        new A.Extents() { Cx = width, Cy = height }
                    ),
                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }
                ),
                new TextBody(
                    new A.BodyProperties(),
                    new A.ListStyle(),
                    new A.Paragraph(
                        new A.ParagraphProperties(
                            new A.DefaultRunProperties(
                                new A.SolidFill(
                                    new A.RgbColorModelHex() { Val = fontColor }
                                )
                            )
                        ),
                        new A.Run(
                            new A.RunProperties(
                                new A.LatinFont() { Typeface = fontFamaly }
                            )
                            {
                                FontSize = fontSize,
                                Bold = false,
                                Language = "ru-RU"
                            },
                            new A.Text(text)
                        )
                    )
                )
            );

            shapeTree.AppendChild(textShape);
            slidePart.Slide.Save();
        }
        
        public void AddTable(SlidePart slidePart, int rowsCount, int columnsCount, 
            long x = 500, long y = 200, long width = 500, long height = 100,
            List<List<TextTableModel>> cellTexts = null, int headerRowIndex = -1)
        {
            // Конвертируем пиксели в EMU
            x *= emuPerPixel;
            y *= emuPerPixel;
            width *= emuPerPixel;
            height *= emuPerPixel;
    
            var shapeTree = slidePart.Slide.CommonSlideData.ShapeTree;
            if (shapeTree == null)
            {
                shapeTree = new ShapeTree();
                slidePart.Slide.CommonSlideData.ShapeTree = shapeTree;
            }
    
            uint nextId = GetNextShapeId(shapeTree);
    
            // 1. Создаем графическую фреймворк для таблицы
            var graphicFrame = new GraphicFrame(
                new NonVisualGraphicFrameProperties(
                    new NonVisualDrawingProperties() { Id = nextId, Name = $"Table_{nextId}" },
                    new NonVisualGraphicFrameDrawingProperties(),
                    new ApplicationNonVisualDrawingProperties()
                ),
                new Transform(
                    new A.Offset() { X = x, Y = y },
                    new A.Extents() { Cx = width, Cy = height }
                ),
                new A.Graphic(
                    new A.GraphicData(
                            CreateTable(rowsCount, columnsCount, cellTexts, headerRowIndex)
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/table" }
                )
            );
    
            shapeTree.AppendChild(graphicFrame);
            slidePart.Slide.Save();
        }
        
        
        private A.Table CreateTable(int rowsCount, int columnsCount, List<List<TextTableModel>> cellTexts, int headerRowIndex)
        {
            var table = new A.Table();

            var tableProperties = new A.TableProperties() { FirstRow = true, BandRow = true };
            // var tableStyleId = new A.TableStyleId();
            // tableStyleId.Text = "{5C1824F3-D215-4745-939A-875A79551049}";
            // tableProperties.Append(tableStyleId);
            table.Append(tableProperties);

            // =========================
            // 📏 АВТОШИРИНА КОЛОНОК
            // =========================
            var colWidths = new long[columnsCount];

            for (int col = 0; col < columnsCount; col++)
            {
                int maxLen = 5;

                for (int row = 0; row < rowsCount; row++)
                {
                    if (cellTexts != null && row < cellTexts.Count && col < cellTexts[row].Count)
                    {
                        var text = cellTexts[row][col]?.value.Split('\n')[0] ?? "";
                        if (text.Length > maxLen)
                            maxLen = text.Length;
                    }
                }

                // магический коэффициент (подбирается)
                colWidths[col] = maxLen * 190000;
            }

            // =========================
            // GRID
            // =========================
            var tableGrid = new A.TableGrid();

            for (int col = 0; col < columnsCount; col++)
            {
                tableGrid.Append(new A.GridColumn() { Width = colWidths[col] });
            }

            table.Append(tableGrid);

            // =========================
            // СТРОКИ
            // =========================
            for (int row = 0; row < rowsCount; row++)
            {
                var tableRow = new A.TableRow() { Height = 400000 }; ;

                for (int col = 0; col < columnsCount; col++)
                {
                    var cell = new A.TableCell();

                    TextTableModel current = null;
                    if (cellTexts != null && row < cellTexts.Count && col < cellTexts[row].Count)
                    {
                        current = cellTexts[row][col];
                    }

                    string text = current?.value ?? "";

                    // =========================
                    // 🎨 ТЕКСТ
                    // =========================
                    var runProps = new A.RunProperties()
                    {
                        FontSize = (current?.fontSize ?? 12) * 100,
                        Bold = (headerRowIndex == row)
                    };

                    if (!string.IsNullOrEmpty(current?.fontColor))
                    {
                        runProps.Append(new A.SolidFill(
                            new A.RgbColorModelHex() { Val = current.fontColor.Replace("#", "") }
                        ));
                    }

                    var run = new A.Run(runProps, new A.Text(text));

                    // =========================
                    // ↔ ВЫРАВНИВАНИЕ
                    // =========================
                    var paragraph = new A.Paragraph();
                    var paraProps = new A.ParagraphProperties();

                    if (current?.center != null)
                    {
                        switch (current.center.ToLower())
                        {
                            case "center":
                                paraProps.Alignment = A.TextAlignmentTypeValues.Center;
                                break;
                            case "right":
                                paraProps.Alignment = A.TextAlignmentTypeValues.Right;
                                break;
                            default:
                                paraProps.Alignment = A.TextAlignmentTypeValues.Left;
                                break;
                        }
                    }
                    
                    paragraph.Append(paraProps);
                    
                    paragraph.Append(run);
                    paragraph.Append(new A.EndParagraphRunProperties() { Language = "ru-RU" });

                    var textBody = new A.TextBody(
                        new A.BodyProperties(),
                        new A.ListStyle(),
                        paragraph
                    );

                    cell.Append(textBody);

                    // =========================
                    // 🎨 ФОН ЯЧЕЙКИ
                    // =========================
                    var cellProps = new A.TableCellProperties() 
                    { 
                        Anchor = A.TextAnchoringTypeValues.Center 
                    };
                    
                    cellProps.Append(new A.LeftBorderLineProperties(new A.NoFill()));
                    cellProps.Append(new A.RightBorderLineProperties(new A.NoFill()));
                    cellProps.Append(new A.TopBorderLineProperties(new A.NoFill()));
                    cellProps.Append(new A.BottomBorderLineProperties(new A.NoFill()));

                    if (!string.IsNullOrEmpty(current?.backgroundColor))
                    {
                        cellProps.Append(new A.SolidFill(
                            new A.RgbColorModelHex() { Val = current.backgroundColor.Replace("#", "") }
                        ));
                    }
                    

                    cell.Append(cellProps);

                    tableRow.Append(cell);
                }

                table.Append(tableRow);
            }

            return table;
        } 

        
        private uint GetNextShapeId(ShapeTree shapeTree)
        {
            if (shapeTree.ChildElements.Count == 0)
                return 1;
                
            var maxId = shapeTree.ChildElements
                .OfType<Shape>()
                .Select(s => s.NonVisualShapeProperties?.NonVisualDrawingProperties?.Id?.Value)
                .Where(id => id.HasValue)
                .Max() ?? 0;
                
            return maxId + 1;
        }
        
        
        public async Task<string> AddImageFromUrlAsync(SlidePart slidePart, string imageUrl, Int64Value _X, Int64Value _Y, 
        long width, CancellationToken token)
        {
            _X *= emuPerPixel;
            _Y *= emuPerPixel;
            try
            {
                token.ThrowIfCancellationRequested();
                var imageType = ImagePartType.Jpeg;
                var imagePart = slidePart.AddImagePart(imageType);
                
                token.ThrowIfCancellationRequested();
                using (var httpClient = new HttpClient())
                {
                    token.ThrowIfCancellationRequested();
                    httpClient.Timeout = TimeSpan.FromSeconds(30);
         
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                    httpClient.DefaultRequestHeaders.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
                    httpClient.DefaultRequestHeaders.Add("Referer", "https://www.google.com/");
                
                    using (var stream = await httpClient.GetStreamAsync(imageUrl))
                    {
                        token.ThrowIfCancellationRequested();
                        imagePart.FeedData(stream);
                    }
                }
                
                token.ThrowIfCancellationRequested();
                
                var relId = slidePart.GetIdOfPart(imagePart);
                var tree = slidePart.Slide.CommonSlideData.ShapeTree;
                var height = await ImageSizeGetter.GetImageHeightAsync(imageUrl, width);
                
                if (height > width)
                    throw new Exception();
                
                token.ThrowIfCancellationRequested();
                
                var picture = CreatePicture(relId, _X, _Y, width, height, 
                    (uint)(tree.ChildElements.Count + 1));
                
                token.ThrowIfCancellationRequested();
                
                tree.AppendChild(picture);
                RemoveUnwantedPlaceholders(slidePart);
                
                token.ThrowIfCancellationRequested();

                return imageUrl;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Загрузка изображения {imageUrl} была отменена");
                throw; 
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при добавлении изображения: {ex.Message}", ex);
            }
        }



    // Вспомогательный метод для создания картинки
    private Picture CreatePicture(string relId, Int64Value x, Int64Value y, 
        long width, long height, uint id)
    {

        return new Picture(
            new NonVisualPictureProperties(
                new NonVisualDrawingProperties()
                {
                    Id = id,
                    Name = $"Picture_{id}",
                    Description = "Изображение из интернета"
                },
                new NonVisualPictureDrawingProperties(),
                new ApplicationNonVisualDrawingProperties()
            ),
            new BlipFill(
                new A.Blip()
                {
                    Embed = relId,
                    CompressionState = A.BlipCompressionValues.Print
                },
                new A.Stretch(new A.FillRectangle())
            ),
            new ShapeProperties(
                new A.Transform2D(
                    new A.Offset() { X = x, Y = y },
                    new A.Extents() 
                    { 
                        Cx = width * emuPerPixel, 
                        Cy = height * emuPerPixel 
                    }
                ),
                new A.PresetGeometry(new A.AdjustValueList())
                {
                    Preset = A.ShapeTypeValues.Rectangle
                }
            )
        );
    }

        public static void AddToSlideList(PresentationPart presPart, SlidePart slidePart)
        {
            if (presPart.Presentation.SlideIdList == null)
            {
                presPart.Presentation.SlideIdList = new SlideIdList();
            }

            var list = presPart.Presentation.SlideIdList;

            uint newId = 256U;

            if (list.ChildElements.Count > 0)
            {
                newId = list.ChildElements
                    .OfType<SlideId>()
                    .Max(s => s.Id.Value) + 1;
            }

            list.Append(new SlideId()
            {
                Id = newId,
                RelationshipId = presPart.GetIdOfPart(slidePart)
            });

            presPart.Presentation.Save();
        }
        
        public void RemoveUnwantedPlaceholders(SlidePart slidePart)
        {
            var shapes = slidePart.Slide.CommonSlideData.ShapeTree
                .Elements<Shape>()
                .ToList();

            foreach (var shape in shapes)
            {
                var placeholder = shape.NonVisualShapeProperties?
                    .ApplicationNonVisualDrawingProperties?
                    .GetFirstChild<PlaceholderShape>();

                if (placeholder == null)
                    continue;

                var type = placeholder.Type?.Value;
                
                if (type == PlaceholderValues.DateAndTime ||
                    type == PlaceholderValues.Footer ||
                    type == PlaceholderValues.SlideNumber ||
                    type == PlaceholderValues.SubTitle)
                {
                    shape.Remove();
                }
            }
        }
}