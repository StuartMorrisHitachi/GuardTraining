using Azure.AI.OpenAI;
using Azure;

Console.WriteLine("Hello, Bot!");
Shared s = new();

string inputText = "I had the best day of my life. I wish you were there with me.";

OpenAIClient openAIClient = new OpenAIClient(
    new Uri(s.GetEnvironment("azureOpenAIEndpoint")),
    new AzureKeyCredential(s.GetEnvironment("azureOpenAIEndpointKey"))
);
ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions();
chatCompletionsOptions.Temperature = 0.7f;
chatCompletionsOptions.DeploymentName = s.GetEnvironment("deploymentNamegpt4");
chatCompletionsOptions.Messages.Add(new ChatRequestSystemMessage(@"You are a text sentiment analysis bot
TASK
* Read the text content provided
* For every piece of text you read, you must provide a score from -1 to 1 regarding how positive or negative the sentiment of the text is
* 1 is entirely positive
* -1 is entirely negative
* Provide a score and a short (10-20 word) justification for your score

Take your time thinking through the research before responding.
"));
foreach (string sentence in inputText.Split(".")) {
    if (sentence.Trim().Length > 0) {
        Console.WriteLine("Bot is analyzing the following text: " + sentence);
        chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Your input text is: " + sentence));
        var completionResponse = openAIClient.GetChatCompletions(chatCompletionsOptions);
        string processedText = completionResponse.Value.Choices[0].Message.Content;
        Console.WriteLine(processedText);
    }
}

