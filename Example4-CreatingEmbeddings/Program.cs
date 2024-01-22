
using System.Text;
using System.Xml;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.Sqlite;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

Console.WriteLine("Hello, Bot!");
Shared s = new();

StringBuilder text = new StringBuilder();
using (PdfDocument document = PdfDocument.Open(s.GetEnvironment("annualReportPDF"))) {
    foreach (Page page in document.GetPages()) {
        foreach (Word word in page.GetWords()) {
            text.Append(word.Text + " ");
        }
    }
}

List<string> chunksList = new List<string>();
// Break our string down into 2500 character chunks with 25 character overlap
int length = 2500;
int overlap = 25;

for (int i = 0; i < text.Length;) {
    int end = i + length <= text.Length ? i + length : text.Length;
    chunksList.Add(text.ToString().Substring(i, end - i));

    // Move to the next chunk, making sure to overlap the last 25 characters of the current chunk
    i = end - overlap;

    // If the end of the text has been reached, break the loop
    if (end == text.Length)
        break;
}

Console.WriteLine("Chunks: " + chunksList.Count);
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

int chunkCounter = 1;
Guid simulatedDocumentId = Guid.NewGuid();
while (chunkCounter < chunksList.Count) {
    string question = chunksList[chunkCounter];
    JObject metaDataInfo = new JObject {
        { "originalfile", Path.GetFileName(s.GetEnvironment("annualReportPDF")) },
        { "chunknumber", chunkCounter }
    };

    string metadataJson = JsonConvert.SerializeObject(metaDataInfo, Newtonsoft.Json.Formatting.Indented);
    while (true) {
        string chunkText = chunksList[chunkCounter];
        
        Console.WriteLine("PROCESSING TEXT " + chunkCounter + Environment.NewLine);
        try {

            //use SaveReferenceAsync to exclude text from the DB
            string result = await memoryWithCustomDb.SaveInformationAsync(
                collection: s.GetEnvironment("documentCollectionName"),
                description: "",
                text: chunkText,
                id: simulatedDocumentId.ToString() + "|" + chunkCounter,
                additionalMetadata: metadataJson
            );
            break;
        }
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
            //the service can be busy so a delay retry logic is a good idea
            await System.Threading.Tasks.Task.Delay(1000);
        }
    }

    chunkCounter++;
}
