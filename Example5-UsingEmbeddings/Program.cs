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
        Console.WriteLine(mqr.Relevance);
        if (mqr.Embedding.Value.Length > 0) {
            float[] array = mqr.Embedding.Value.ToArray();
            Console.Write("EMBEDDING: [");
            for (int i = 0; i < array.Length; i++) {
                Console.Write(" " + array[i] + " ");
            }
            Console.WriteLine("]");
        }
        Console.WriteLine(mqr.Metadata.Text);
        Console.WriteLine("----------");
    }
#pragma warning restore SKEXP0003
}
catch (Exception ex) {
    Console.WriteLine(ex.Message);
}
