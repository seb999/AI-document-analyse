using Azure.AI.OpenAI;
using Azure;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace EwrsDocAnalyses.Misc
{
    public class ChatGptEngine2
    {
        private const string GPT4V_KEY = "3a401efe61c64348ae3d34d2cf217ebb"; // Set your key here
        private const string IMAGE_PATH = @"C:/Users/sdubos/Downloads/_ImagesClassification/Image2.jpg"; // Set your image path here
        private const string GPT4V_ENDPOINT = "https://sebastiencopilot-aiservices.openai.azure.com/openai/deployments/gpt-4/extensions/chat/completions?api-version=2023-07-01-preview";

        public async Task<string> AskQuestion(List<object> messages, bool isEpi, string imageUrl)
        {
            //var encodedImage = Convert.ToBase64String(File.ReadAllBytes(imageUrl));
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("api-key", GPT4V_KEY);
                
                var payload = new
                {
                        // new {
                        //     role = ChatRole.User,
                        //     content = new object[] {
                        //         new {
                        //             type = "text",
                        //             text =  "what animal is visible on this image"
                        //         },
                        //         new {
                        //             type = "image_url",
                        //             image_url = new {
                        //                 url = $"data:image/jpeg;base64,{encodedImage}"
                        //             }
                        //         }
                        //     }
                        // }
                   
                    messages = messages,
                    temperature = 0.7,
                    top_p = 0.95,
                    max_tokens = 2000,
                    stream = false
                };

                var response = await httpClient.PostAsync(GPT4V_ENDPOINT, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<ChatCompletionResult>(await response.Content.ReadAsStringAsync());
                    foreach (var item in responseData.Choices)
                    {
                        if(item.Message.Role == "assistant") return item.Message.Content;
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                }

                return response.ToString();
            }
        }
    }
}
