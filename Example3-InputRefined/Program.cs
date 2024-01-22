// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Xml;
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

List<string> chunksList = new List<string>();
//break our string down into 6000 character chunks
int length = 6000;
for (int i = 0; i < text.ToString().Length; i += length) {
    if (i + length > text.ToString().Length) length = text.ToString().Length - i;
    chunksList.Add(text.ToString().Substring(i, length));
}

Console.WriteLine("Chunks: " + chunksList.Count);

int chunkCounter = 1;
foreach (string chunk in chunksList) {
    ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions();
    //temperature from 0 to 2.0 with 2 being the most random/innovative.  0.7 is pretty standard, mildly creative. over 1.4 is highly unstable
    chatCompletionsOptions.Temperature = 0.6f;
    //the bot can return multiple top responses here in one call
    //chatCompletionsOptions.ChoiceCount = 10;
    chatCompletionsOptions.DeploymentName = s.GetEnvironment("deploymentNamegpt4");
    chatCompletionsOptions.Messages.Add(new ChatRequestSystemMessage(@"
You are a helpful text analysis bot.  
You will be supplied with a wikipedia article and a specific item or person to research.  
Respond with a bulleted list of everything you know about that item or person based only on the research provided.
If you don't have any information on the topic, you must repond 'NONE'
"));
    chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Here is your research: " + chunk));
    chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage("Understood waiting on the item to research"));
    chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Stuart Morris"));
    //chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Falls Park Bridge"));

    var completionResponse = openAIClient.GetChatCompletions(chatCompletionsOptions);
    string processedText = completionResponse.Value.Choices[0].Message.Content;

    Console.WriteLine(chunkCounter++ + ": " + processedText);
}
