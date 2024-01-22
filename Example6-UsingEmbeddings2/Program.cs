// See https://aka.ms/new-console-template for more information
using System.Text;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.Sqlite;
using Microsoft.SemanticKernel.Connectors.OpenAI;

Console.WriteLine("Hello, Bot!");
Shared s = new();

#pragma warning disable SKEXP0028 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0011
var sqliteStore = await SqliteMemoryStore.ConnectAsync(s.GetEnvironment("sqliteDB"));
var memoryWithCustomDb = new MemoryBuilder()
    .WithAzureOpenAITextEmbeddingGeneration(
        deploymentName: s.GetEnvironment("deploymentNameEmbedding"),
        endpoint: s.GetEnvironment("azureOpenAIEndpoint"),
        apiKey: s.GetEnvironment("azureOpenAIEndpointKey"),
        modelId: s.GetEnvironment("deploymentNameEmbedding")
    )
    .WithMemoryStore(sqliteStore)
    .Build();
#pragma warning restore SKEXP0028 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning restore SKEXP0003
#pragma warning restore SKEXP0011


string inputQueryText = "status of the aerospace division";
StringBuilder researchCombinedForBot = new StringBuilder();
try {

    var result = memoryWithCustomDb.SearchAsync(
        collection: s.GetEnvironment("documentCollectionName"),
        query: inputQueryText,
        limit: 5,
        minRelevanceScore: 0.70,
        withEmbeddings: false
    );
    
#pragma warning disable SKEXP0003
    int resultCounter = 1;
    await foreach (MemoryQueryResult mqr in result) {
        Console.WriteLine("RESULT #" + resultCounter++ + ":");
        researchCombinedForBot.AppendLine(mqr.Metadata.Id + ":::" + mqr.Metadata.Text);
    }
#pragma warning restore SKEXP0003
}
catch (Exception ex) {
    Console.WriteLine(ex.Message);
}

OpenAIClient openAIClient = new OpenAIClient(
    new Uri(s.GetEnvironment("azureOpenAIEndpoint")),
    new AzureKeyCredential(s.GetEnvironment("azureOpenAIEndpointKey"))
);
ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions();
chatCompletionsOptions.Temperature = 0.7f;
chatCompletionsOptions.DeploymentName = s.GetEnvironment("deploymentNamegpt4");
chatCompletionsOptions.Messages.Add(new ChatRequestSystemMessage(@"You are a helpful research bot. 
TASK
* Read research and provide a thoughtful response to the users query with citations
* Perform a complete synthesis of the research provided
* Cite the research you when making a statement by outputting the citation id in square brackets after your statement
* The citation id always precedes the research and is delimited by :::

EXAMPLE 1
23ab49e3-ad3f-f039-de411f984:::useful source research information here
your output may look like
My synthesized statement that draws from this research [23ab49e3-ad3f-f039-de411f984]

RULES
* RESPOND with well formatted HTML only
* You must cite every statement that you make
* You mostly utilize the research that is supplied to you for your response.  Do not cite information from memory

Take your time thinking through the research before responding.


"));
chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Here is the research information\n\n" + researchCombinedForBot.ToString()));
chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage("Understood. Awaiting the user's research request."));
chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage("Your research request is: " + inputQueryText));

var completionResponse = openAIClient.GetChatCompletions(chatCompletionsOptions);
string processedText = completionResponse.Value.Choices[0].Message.Content;
Console.WriteLine(processedText);