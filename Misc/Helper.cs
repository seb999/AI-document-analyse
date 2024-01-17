using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Xobject;



namespace EwrsDocAnalyses.Misc
{


    public static class Helper
    {
        static string documentDirectory = "EwrsDocuments";
        static string imageDirectory = "EwrsImages";

        public static string ExtractDocx(string docxFilePath)
        {
            try
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Open(docxFilePath, false))
                {
                    MainDocumentPart mainPart = doc.MainDocumentPart;
                    if (mainPart != null)
                    {
                        var paragraphs = mainPart.Document.Descendants<Paragraph>().Take(200);
                        string text = string.Join(Environment.NewLine, paragraphs.Select(p => p.InnerText));
                        return text;
                    }
                    else
                    {
                        Console.WriteLine("The document does not contain a main part.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading DOCX file: {ex.Message}");
            }

            return null;
        }

        public static string ExtractPdf(string pdfFilePath)
        {
            Toto(pdfFilePath);
            if (!File.Exists(pdfFilePath))
            {
                throw new FileNotFoundException("The specified PDF file does not exist.");
            }
            using (PdfReader pdfReader = new PdfReader(pdfFilePath))
            {
                using (var textWriter = new StringWriter())
                {
                    PdfDocument pdfDocument = new PdfDocument(pdfReader);
                    for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
                    {
                        var strategy = new LocationTextExtractionStrategy();
                        string currentText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);

                        // Split the text into lines and append the first 20 lines
                        string[] lines = currentText.Split('\n');
                        for (int i = 0; i < Math.Min(lines.Length, 20); i++)
                        {
                            textWriter.WriteLine(lines[i]);
                        }
                    }

                    return textWriter.ToString();
                }
            }
        }

        public static void Toto(string pdfFilePath)
        {
            try
            {
                using (PdfReader pdfReader = new PdfReader(pdfFilePath))
                using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
                {
                    for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
                    {
                        PdfPage page = pdfDocument.GetPage(pageNum);
                        PdfResources resources = page.GetResources();

                        // Get the XObject dictionary from the Resources
                        PdfDictionary xObjects = resources.GetResource(PdfName.XObject);

                        if (xObjects != null)
                        {
                            int imageCount = 1;
                            foreach (PdfName key in xObjects.KeySet())
                            {
                                PdfStream pdfStream = xObjects.GetAsStream(key);
                                PdfImageXObject imageXObject = new PdfImageXObject(pdfStream);

                                byte[] imageBytes = imageXObject.GetImageBytes();
                                SaveImageToFile(imageBytes, $"image{imageCount}.png");

                                imageCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void SaveImageToFile(byte[] imageBytes, string fileName)
        {
            try
            {
                if (!Directory.Exists(imageDirectory))
                {
                    Directory.CreateDirectory(imageDirectory);
                }
                string fileFullPath = Path.Combine(imageDirectory, $"image{fileName}.png");
                File.WriteAllBytes(fileFullPath, imageBytes);
                Console.WriteLine($"Image saved: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image {fileName}: {ex.Message}");
            }
        }
    }
}