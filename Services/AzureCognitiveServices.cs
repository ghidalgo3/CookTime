using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace babe_algorithms;

public class AzureCognitiveServices : IComputerVision
{
    public ComputerVisionClient Client { get; }

    public AzureCognitiveServices(AzureOptions options)
    {
        this.Client = Authenticate(options.VisionEndpoint, options.VisionKey);
    }

    public async Task<string> GetTextAsync(Stream image, CancellationToken ct)
    {
        var headers = await this.Client.ReadInStreamAsync(image);
        string operationLocation = headers.OperationLocation;
        await Task.Delay(1000, ct);
        // <snippet_extract_response>
        // Retrieve the URI where the recognized text will be stored from the Operation-Location header.
        // We only need the ID and not the full URL
        const int numberOfCharsInOperationId = 36;
        string operationId = operationLocation[^numberOfCharsInOperationId..];

        // Extract the text
        ReadOperationResult results;
        do
        {
            results = await this.Client.GetReadResultAsync(Guid.Parse(operationId), ct);
        }
        while ((results.Status == OperationStatusCodes.Running ||
            results.Status == OperationStatusCodes.NotStarted));
        var textUrlFileResults = results.AnalyzeResult.ReadResults;

        return string.Join(" ", textUrlFileResults.SelectMany(result => result.Lines.Select(line => line.Text)));
    }

    private static ComputerVisionClient Authenticate(string endpoint, string key)
    {
        ComputerVisionClient client =
          new(new ApiKeyServiceClientCredentials(key))
          { Endpoint = endpoint };
        return client;
    }
}