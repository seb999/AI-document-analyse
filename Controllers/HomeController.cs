using Microsoft.AspNetCore.Mvc;
using EwrsDocAnalyses.Misc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using EwrsDocAnalyses.Class;
using System.Reflection;
using DocumentFormat.OpenXml.Spreadsheet;
using Azure.AI.OpenAI;

namespace EwrsDocAnalyses.Controllers;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    string questionsPath = "questionForChatGPT.json";
    string documentDirectory = "EwrsDocuments";
    private readonly FolderWatcherService _folderWatcherService;
    private readonly ILogger<HomeController> _logger;
    private readonly IHubContext<SignalRHub> _hub;

    public HomeController(ILogger<HomeController> logger, IHubContext<SignalRHub> hub, FolderWatcherService folderWatcherService)
    {
        _logger = logger;
        _folderWatcherService = folderWatcherService;
        _hub = hub;
    }

    [HttpGet("[action]")]
    public IActionResult GetDocuments()
    {
        List<string> pdfAndDocxFiles = new List<string>();

        if (!Directory.Exists(documentDirectory))
        {
            throw new DirectoryNotFoundException("The specified folder does not exist.");
        }

        string[] pdfFiles = Directory.GetFiles(documentDirectory, "*.pdf");
        string[] docxFiles = Directory.GetFiles(documentDirectory, "*.docx");
        string[] excelFiles = Directory.GetFiles(documentDirectory, "*.xlsx");

        // Extract only the file names without the path
        foreach (string file in excelFiles)
        {
            pdfAndDocxFiles.Add(Path.GetFileName(file));
        }

        foreach (string file in pdfFiles)
        {
            pdfAndDocxFiles.Add(Path.GetFileName(file));
        }

        foreach (string file in docxFiles)
        {
            pdfAndDocxFiles.Add(Path.GetFileName(file));
        }
        return Ok(pdfAndDocxFiles);
    }

    [HttpPost("[action]")]
    public async Task<MetaData> GetMetadata([FromBody] string document, bool isEpi)
    {
        MetaData metaData = new MetaData();
        ChatGptEngine chatGpt = new ChatGptEngine();
        string myQuestion = "";

        List<Prompt> prompList = JsonHelper.ParseJsonAndGetList(questionsPath);
        for (int i = 0; i < prompList.Count; i++)
        {
            //Build the question
            if (prompList[i].Sublist.Count > 0)
            {
                myQuestion = $"{document}\n{prompList[i].Question}\n{string.Join(", ", prompList[i].Sublist)}";
            }
            else
            {
                myQuestion = $"{document}\n{prompList[i].Question}\n";
            }

            //get the property from return object that correspond to item.key
            PropertyInfo? propertyInfo = typeof(MetaData).GetProperty(prompList[i].Key);
            if (propertyInfo != null)
            {
                string chatgptResponse = await chatGpt.AskQuestion(myQuestion, isEpi);
                //Set to the property the result
                propertyInfo?.SetValue(metaData, chatgptResponse);
            }
        }

        //Return the object result to UI
        return metaData;
    }

    [HttpGet("[action]/{isEpi}")]
    public async void GetDocumentMetadata(bool isEpi)
    {
        string myExcelName = $"{documentDirectory}/PredictionResult-IsEpi-{isEpi}.xlsx";
        if (Directory.Exists(documentDirectory))
        {
            //Create the excel Result file
            ExcelHelper.CreateExcelFile(myExcelName, questionsPath);

            //Filter docx document
            string[] docxFiles = Directory.GetFiles(documentDirectory, "*.docx");
            foreach (string docxFile in docxFiles)
            {
                var sample = Helper.ExtractDocx(docxFile);

                //New techniques to test
                var metaData = await GeneratePayloadMessages(sample);

                //var metaData = await GetMetadata(sample, isEpi);

                metaData.FileName = docxFile.Split("\\")[docxFile.Split("\\").Length - 1];

                ExcelHelper.AddMetaDataToExcel(myExcelName, metaData);
            }

            //Filter pdf documents
            string[] pdfFiles = Directory.GetFiles(documentDirectory, "*.pdf");
            foreach (string pdfFile in pdfFiles)
            {
                var sample = Helper.ExtractPdf(pdfFile);

                var metaData = await GetMetadata(sample, isEpi);

                metaData.FileName = pdfFile.Split("\\")[pdfFile.Split("\\").Length - 1];

                ExcelHelper.AddMetaDataToExcel(myExcelName, metaData);
            }
        }
        else
        {
            Console.WriteLine("The specified folder does not exist.");
        }

        _hub.Clients.All.SendAsync("chatGptDone", JsonSerializer.Serialize("ok"));
    }

    public async Task<MetaData> GeneratePayloadMessages(string document)
    {
        MetaData metaData = new MetaData();
        ChatGptEngine2 chatGpt2 = new ChatGptEngine2();
        var messages = new List<object>();

        messages.Add(new
        {
            role = "system",
            content = new object[]
            {
                new
                {
                    type = "text",
                    text = "You're an epidemiologist, and all your answers have to be in the field of epidemiology. You are trained to interpret images about people or epidemiology and make responsible assumptions about them."
                }
            }
        });
        messages.Add(new
        {
            role = "user",
            content = new object[]
            {
                new
                {
                    type = "text",
                    text = "Source Text: " + document
                }
            }
        });

        List<Prompt> prompList = JsonHelper.ParseJsonAndGetList(questionsPath);
        for (int i = 0; i < prompList.Count; i++)
        {
            //Build the question
            if (prompList[i].Sublist.Count > 0)
            {
                messages.Add(new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = $"{prompList[i].Question}\n{string.Join(", ", prompList[i].Sublist)}"
                        }
                    }
                });
            }
            else
            {
                messages.Add(new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = $"{prompList[i].Question}\n"
                        }
                    }
                });
            }

            //for debug
            //string chatgptResponse = await chatGpt2.AskQuestion(messages, false, "");

            //get the property from return object that correspond to item.key
            PropertyInfo? propertyInfo = typeof(MetaData).GetProperty(prompList[i].Key);
            if (propertyInfo != null)
            {
                string chatgptResponse = await chatGpt2.AskQuestion(messages, false, "");
                //Set to the property the result
                propertyInfo?.SetValue(metaData, chatgptResponse);
            }
        }
          //Return the object result to UI
        return metaData;
    }
}