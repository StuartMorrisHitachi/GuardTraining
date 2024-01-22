
using Azure;
using Azure.AI.TextAnalytics;

Console.WriteLine("Hello, Bot!");
Shared s = new();

AzureKeyCredential credentials = new AzureKeyCredential(s.GetEnvironment("azureLanguageServicesKey"));
Uri endpoint = new Uri(s.GetEnvironment("azureLanguageServicesEndpoint"));

var textAnalyticsClient = new TextAnalyticsClient(endpoint, credentials);

string inputText = "I had the best day of my life. I wish you were there with me.";
DocumentSentiment documentSentiment = textAnalyticsClient.AnalyzeSentiment(inputText);
Console.WriteLine($"Document sentiment: {documentSentiment.Sentiment}\n");

foreach (var sentence in documentSentiment.Sentences) {
    Console.WriteLine($"\tText: \"{sentence.Text}\"");
    Console.WriteLine($"\tSentence sentiment: {sentence.Sentiment}");
    Console.WriteLine($"\tPositive score: {sentence.ConfidenceScores.Positive:0.00}");
    Console.WriteLine($"\tNegative score: {sentence.ConfidenceScores.Negative:0.00}");
    Console.WriteLine($"\tNeutral score: {sentence.ConfidenceScores.Neutral:0.00}\n");
}
