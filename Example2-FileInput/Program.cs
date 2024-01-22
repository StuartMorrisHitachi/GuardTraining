using System.Text;
using Azure.AI.OpenAI;
using Azure;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;

Console.WriteLine("Hello, Bot!");
Shared s = new();

StringBuilder text = new StringBuilder();
using (PdfDocument document = PdfDocument.Open(s.GetEnvironment("samplePDF"))) {
    foreach (Page page in document.GetPages()) {
        foreach (Word word in page.GetWords()) {
            text.Append(word.Text + " ");
        }
    }
}

OpenAIClient openAIClient = new OpenAIClient(
    new Uri(s.GetEnvironment("azureOpenAIEndpoint")),
    new AzureKeyCredential(s.GetEnvironment("azureOpenAIEndpointKey"))
);
ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions();

//temperature from 0 to 2.0 with 2 being the most random/innovative.  0.7 is pretty standard, mildly creative. over 1.4 is highly unstable
chatCompletionsOptions.Temperature = 0.6f;
//the bot can return multiple top responses here in one call
//chatCompletionsOptions.ChoiceCount = 10;
chatCompletionsOptions.DeploymentName = s.GetEnvironment("deploymentNamegpt432k");
chatCompletionsOptions.Messages.Add(new ChatRequestSystemMessage(@"
You are a helpful text analysis bot.  
You will be supplied with a wikipedia article and a specific item or person to research.  
Respond with a bulleted list of everything you know about that item or person based only on the research provided
"));
chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Here is your research: " + text.ToString()));
chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage("Understood waiting on the item to research"));
chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Stuart Morris"));
//chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Falls Park Bridge"));

var completionResponse = openAIClient.GetChatCompletions(chatCompletionsOptions);
string processedText = completionResponse.Value.Choices[0].Message.Content;

Console.WriteLine(string.Join("\n", processedText));
