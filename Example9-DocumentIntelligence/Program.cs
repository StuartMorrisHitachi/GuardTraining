
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Text;

Console.WriteLine("Hello, Bot!");
Shared s = new();

AzureKeyCredential credentials = new AzureKeyCredential(s.GetEnvironment("azureDocumentIntelligenceKey"));
Uri endpoint = new Uri(s.GetEnvironment("azureDocumentIntelligenceEndpoint"));

var client = new DocumentAnalysisClient(endpoint, credentials);

using FileStream fs = new FileStream(s.GetEnvironment("handwrittenFormImage"), FileMode.Open);

var operation = client.AnalyzeDocument(WaitUntil.Completed, "prebuilt-read", fs);

AnalyzeResult result = operation.Value;

StringBuilder fileResultsStringBuilder = new StringBuilder();
AnalyzeResult analyzeOperationResult = operation.Value;
foreach (DocumentPage pageres in analyzeOperationResult.Pages) {
    for (int i = 0; i < pageres.Lines.Count; i++) {
        DocumentLine line = pageres.Lines[i];

        fileResultsStringBuilder.AppendLine(line.Content);
    }
}
Console.WriteLine(fileResultsStringBuilder.ToString());



/*
foreach (DocumentPage page in result.Pages) {
    Console.WriteLine($"Document Page {page.PageNumber} has {page.Lines.Count} line(s), {page.Words.Count} word(s),");
    Console.WriteLine($"and {page.SelectionMarks.Count} selection mark(s).");

    for (int i = 0; i < page.Lines.Count; i++) {
        DocumentLine line = page.Lines[i];
        Console.WriteLine($"  Line {i} has content: '{line.Content}'.");

        Console.WriteLine($"    Its bounding box is:");
        Console.WriteLine($"      Upper left => X: {line.BoundingPolygon[0].X}, Y= {line.BoundingPolygon[0].Y}");
        Console.WriteLine($"      Upper right => X: {line.BoundingPolygon[1].X}, Y= {line.BoundingPolygon[1].Y}");
        Console.WriteLine($"      Lower right => X: {line.BoundingPolygon[2].X}, Y= {line.BoundingPolygon[2].Y}");
        Console.WriteLine($"      Lower left => X: {line.BoundingPolygon[3].X}, Y= {line.BoundingPolygon[3].Y}");
    }

    for (int i = 0; i < page.SelectionMarks.Count; i++) {
        DocumentSelectionMark selectionMark = page.SelectionMarks[i];

        Console.WriteLine($"  Selection Mark {i} is {selectionMark.State}.");
        Console.WriteLine($"    Its bounding box is:");
        Console.WriteLine($"      Upper left => X: {selectionMark.BoundingPolygon[0].X}, Y= {selectionMark.BoundingPolygon[0].Y}");
        Console.WriteLine($"      Upper right => X: {selectionMark.BoundingPolygon[1].X}, Y= {selectionMark.BoundingPolygon[1].Y}");
        Console.WriteLine($"      Lower right => X: {selectionMark.BoundingPolygon[2].X}, Y= {selectionMark.BoundingPolygon[2].Y}");
        Console.WriteLine($"      Lower left => X: {selectionMark.BoundingPolygon[3].X}, Y= {selectionMark.BoundingPolygon[3].Y}");
    }
}
*/