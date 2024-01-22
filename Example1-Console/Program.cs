using Azure.AI.OpenAI;
using Azure;

Console.WriteLine("Hello, Bot!");
Shared s = new();

OpenAIClient openAIClient = new OpenAIClient(
    new Uri(s.GetEnvironment("azureOpenAIEndpoint")),
    new AzureKeyCredential(s.GetEnvironment("azureOpenAIEndpointKey"))
);
ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions();

//temperature from 0 to 2.0 with 2 being the most random/innovative.  0.7 is pretty standard, mildly creative. over 1.4 is highly unstable
chatCompletionsOptions.Temperature = 0.7f;
//the bot can return multiple top responses here in one call
//chatCompletionsOptions.ChoiceCount = 10;
chatCompletionsOptions.DeploymentName = s.GetEnvironment("deploymentNamegpt4"); 
chatCompletionsOptions.Messages.Add(new ChatRequestSystemMessage(@"You are a helpful translation bot. You will answer the user's question using pirate speak."));
chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Briefly describe the capital of the United States of America"));

var completionResponse = openAIClient.GetChatCompletions(chatCompletionsOptions);
string processedText = completionResponse.Value.Choices[0].Message.Content;

Console.WriteLine(processedText);
