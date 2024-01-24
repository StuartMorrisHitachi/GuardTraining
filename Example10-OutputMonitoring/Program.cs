using Azure.AI.OpenAI;
using Azure;

Console.WriteLine("Hello, Bot!");
Shared s = new();

Dictionary<int, string> input = new Dictionary<int, string>();
Dictionary<int, string> validatedOutput = new Dictionary<int, string>();

input.Add(1, "I had the best day of my life.");
input.Add(2, "I wish you were there with me.");

validatedOutput.Add(1, "Score: 1   Justification: The text expresses a very positive sentiment about having the best day of their life.");
validatedOutput.Add(2, "Sentiment Score: 0.5    Justification: The text expresses a wish for companionship, indicating a slight positive sentiment.");

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

for (int counter = 1; counter <= 2; counter++) {
    string inputSentence = input[counter];
    if (inputSentence.Trim().Length > 0) {
        Console.WriteLine("Bot is analyzing the following text: " + inputSentence);
        chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Your input text is: " + inputSentence));
        var completionResponse = openAIClient.GetChatCompletions(chatCompletionsOptions);
        string processedText = completionResponse.Value.Choices[0].Message.Content;
        Console.WriteLine("Current Output:" + processedText);


        chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(processedText));
        chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Now compare the current output for this task to your previous output.  Grade the similarity of this answer by comparing it to your previous answer.  1 is nearly identical, -1 is completely dissimilar.\n\nHere is your previous answer"));
        chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(validatedOutput[counter]));

        completionResponse = openAIClient.GetChatCompletions(chatCompletionsOptions);
        string comparisonResult = completionResponse.Value.Choices[0].Message.Content;
        Console.WriteLine("Comparison Task:" + comparisonResult);
        Console.WriteLine("-----------------------------------");
    }
}

