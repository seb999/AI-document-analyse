using System.Text.Json;
using DocumentFormat.OpenXml.Drawing;
using EwrsDocAnalyses.Class;

namespace EwrsDocAnalyses.Misc
{
    public static class JsonHelper
    {
        public static List<Prompt> ParseJsonAndGetList(string jsonFilePath)
        {
            try
            {
                string json = File.ReadAllText(jsonFilePath);
                var jsonObject = JsonSerializer.Deserialize<Dictionary<string, Prompt>>(json);

                List<Prompt> itemList = new List<Prompt>();
                foreach (var kvp in jsonObject)
                {
                    itemList.Add(new Prompt
                    {
                        Key = kvp.Key,
                        Question = kvp.Value.Question,
                        Sublist = kvp.Value.Sublist
                    });
                }

                return itemList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new List<Prompt>();
            }
        }
    }
}