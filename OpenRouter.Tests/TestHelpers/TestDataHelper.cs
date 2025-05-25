using System.Text.Json;

namespace OpenRouter.Tests.TestHelpers;

public static class TestDataHelper
{
    private static readonly string TestDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");

    public static string LoadTestData(string fileName)
    {
        var filePath = Path.Combine(TestDataPath, fileName);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Test data file not found: {filePath}");
        
        return File.ReadAllText(filePath);
    }

    public static T LoadTestData<T>(string fileName)
    {
        var json = LoadTestData(fileName);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        }) ?? throw new InvalidOperationException($"Failed to deserialize test data from {fileName}");
    }

    public static HttpResponseMessage CreateJsonResponse(string content, System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
        };
    }

    public static HttpResponseMessage CreateStreamingResponse(string content)
    {
        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, "text/event-stream")
        };
    }
}