using Azure.AI.OpenAI;
using Azure;
using System.Text;

namespace EwrsDocAnalyses.Misc

{
    public class ChatGptEngine
    {
        public ChatGptEngine()
        {

        }

        public async Task<string> AskQuestion(string prompt, bool isEpi)
        {
            string initAI = "You're an epidemiologist and all your answers has to be in the field of epidemiology.";

            var openAiClient = new OpenAIClient(
                //Initial model from Sorin
                new Uri("https://ecdctestazureopenai.openai.azure.com"),
                new AzureKeyCredential("8b122b6da7f34c36971f4b55ba576f67")
            );

            var chatCompletionsOptions = new ChatCompletionsOptions();
            if (isEpi) {
                 chatCompletionsOptions.Messages.Insert(0, new ChatMessage(ChatRole.System, initAI));
            }
           
            chatCompletionsOptions.Messages.Add(
                new ChatMessage(ChatRole.User, prompt));

            chatCompletionsOptions.Temperature = (float)0.7;
            chatCompletionsOptions.MaxTokens = 800;
            chatCompletionsOptions.NucleusSamplingFactor = (float)0.95;
            chatCompletionsOptions.FrequencyPenalty = 0;
            chatCompletionsOptions.PresencePenalty = 0;

            var chatCompletionsResponse = await openAiClient.GetChatCompletionsStreamingAsync(
                "ecdc-test-chat-model",   //---Sorine model
                chatCompletionsOptions
            );

            using StreamingChatCompletions streamingChatCompletions = chatCompletionsResponse.Value;
            StringBuilder response = new StringBuilder();

            await foreach (StreamingChatChoice choice in streamingChatCompletions.GetChoicesStreaming())
            {
                await foreach (ChatMessage message in choice.GetMessageStreaming())
                {
                    response.Append(message.Content);
                }
            }

            return response.ToString();
        }
    }
}
